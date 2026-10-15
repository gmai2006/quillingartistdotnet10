using System.Text;
using System.Text.Json;
using ApiService.Configuration;
using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Services.Communications;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace ApiService.Services;

public class PaymentNotCompletedException(string message) : Exception(message);

public class StripeCheckoutService(
    IOptions<StripeOptions> stripeOptions,
    IOptions<EmailOptions> emailOptions,
    ProductsDao productsDao,
    ProductsService productsService,
    CustomersDao customersDao,
    OrdersDao ordersDao,
    OrderItemsDao orderItemsDao,
    AddressesDao addressesDao,
    EmailDeliveryService emailDeliveryService,
    ILogger<StripeCheckoutService> logger)
{
    private SessionService? sessions;

    private SessionService Sessions => sessions ??= new SessionService(new StripeClient(
        string.IsNullOrWhiteSpace(stripeOptions.Value.SecretKey)
            ? throw new InvalidOperationException("Stripe:SecretKey is not configured")
            : stripeOptions.Value.SecretKey));

    public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CheckoutRequest? request)
    {
        if (request?.Items is null || request.Items.Count == 0)
        {
            throw new ArgumentException("The shopping cart is empty.");
        }

        var lineItems = new List<SessionLineItemOptions>();
        var cartQuantities = new Dictionary<string, double>();

        foreach (var item in request.Items)
        {
            if (item.ProductId is null || item.Quantity is null || item.Quantity <= 0)
            {
                throw new ArgumentException("Each cart item requires a productId and a positive quantity.");
            }

            var product = await productsDao.FindAsync(item.ProductId.Value)
                ?? throw new ArgumentException($"Product {item.ProductId} was not found.");

            var unitAmount = (long)Math.Round(product.Price * 100m, MidpointRounding.AwayFromZero);

            lineItems.Add(new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    UnitAmount = unitAmount,
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = product.Name },
                },
            });

            var key = item.ProductId.Value.ToString();
            cartQuantities[key] = cartQuantities.GetValueOrDefault(key) + item.Quantity.Value;
        }

        var config = stripeOptions.Value;
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = FirstNonBlank(request.SuccessUrl, config.SuccessUrl),
            CancelUrl = FirstNonBlank(request.CancelUrl, config.CancelUrl),
            BillingAddressCollection = "required",
            LineItems = lineItems,
            Metadata = new Dictionary<string, string> { ["cart"] = JsonSerializer.Serialize(cartQuantities) },
        };

        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            options.CustomerEmail = request.CustomerEmail.Trim();
        }
        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            options.Metadata["customerName"] = request.CustomerName.Trim();
        }

        var session = await Sessions.CreateAsync(options);
        logger.LogInformation("createCheckoutSession() - created Stripe session {SessionId}", session.Id);

        return new CheckoutSessionResponse { SessionId = session.Id, Url = session.Url };
    }

    public async Task<CheckoutConfirmResponse> ConfirmCheckoutSessionAsync(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("A Stripe session id is required.");
        }

        var session = await Sessions.GetAsync(sessionId);

        if (session.PaymentStatus != "paid")
        {
            throw new PaymentNotCompletedException(
                $"Stripe reports this payment has not completed (status: {session.PaymentStatus}).");
        }

        var customer = await UpsertCustomerAsync(session);
        var address = await UpsertAddressAsync(customer, session);
        var order = await CreateOrderAsync(session, customer, address);
        var (orderInfo, items) = await CreateOrderItemsAsync(session, order);

        await NotifySupportOfOrderAsync(customer, address, order, orderInfo);

        foreach (var item in items)
        {
            await productsService.UpdateStockNumbersAsync(item.ProductId, item.Quantity);
        }

        return new CheckoutConfirmResponse
        {
            OrderId = order.Id,
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            Status = order.Status,
            Total = order.Total,
        };
    }

    private async Task<Customers> UpsertCustomerAsync(Session session)
    {
        var email = session.CustomerDetails?.Email;
        var name = session.CustomerDetails?.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = session.Metadata?.GetValueOrDefault("customerName") ?? "Guest";
        }

        var customer = await customersDao.GetByEmailAsync(email);
        if (customer is not null)
        {
            return customer;
        }

        var newCustomer = new Customers
        {
            Id = await customersDao.NextIdAsync(),
            Email = email ?? $"guest+{session.Id}@quillingartist.com",
            Name = name,
            CreatedAt = Now(),
        };
        return await customersDao.CreateAsync(newCustomer);
    }

    private async Task<Orders> CreateOrderAsync(Session session, Customers customer, Addresses? address)
    {
        var order = new Orders
        {
            Id = await ordersDao.NextIdAsync(),
            CustomerId = customer.Id,
            ShippingAddressId = address?.Id,
            Status = "paid",
            Total = (session.AmountTotal ?? 0) / 100m,
            CreatedAt = Now(),
        };
        return await ordersDao.CreateAsync(order);
    }

    private async Task<(string OrderInfo, List<OrderItems> Items)> CreateOrderItemsAsync(
        Session session, Orders order)
    {
        var items = new List<OrderItems>();
        var cartJson = session.Metadata?.GetValueOrDefault("cart");
        if (string.IsNullOrWhiteSpace(cartJson))
        {
            return (string.Empty, items);
        }

        var cartQuantities = JsonSerializer.Deserialize<Dictionary<string, double>>(cartJson) ?? [];
        var orderInfo = new StringBuilder();

        foreach (var (productId, quantity) in cartQuantities)
        {
            var product = await productsDao.FindAsync(int.Parse(productId));
            if (product is null)
            {
                continue;
            }

            var orderItem = new OrderItems
            {
                Id = await orderItemsDao.NextIdAsync(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = (int)quantity,
                UnitPrice = product.Price,
            };
            await orderItemsDao.CreateAsync(orderItem);
            items.Add(orderItem);

            orderInfo
                .Append(orderItem.Quantity)
                .Append(" x ")
                .Append(product.Name)
                .Append(" @ $")
                .Append(product.Price)
                .Append(Environment.NewLine);
        }

        return (orderInfo.ToString(), items);
    }

    private async Task<Addresses?> UpsertAddressAsync(Customers customer, Session session)
    {
        var stripeAddress = session.CustomerDetails?.Address;
        if (stripeAddress?.Line1 is null)
        {
            return null;
        }

        var existing = await addressesDao.SelectAllAsync(0, 1, new AddressesFilter(CustomerId: customer.Id));
        var address = existing.FirstOrDefault() ?? new Addresses();
        var isNew = existing.Count == 0;

        address.CustomerId = customer.Id;
        address.Line1 = stripeAddress.Line1;
        address.Line2 = stripeAddress.Line2;
        address.City = stripeAddress.City ?? string.Empty;
        address.State = stripeAddress.State;
        address.PostalCode = stripeAddress.PostalCode ?? string.Empty;
        address.Country = stripeAddress.Country ?? string.Empty;

        if (isNew)
        {
            address.Id = await addressesDao.NextIdAsync();
            return await addressesDao.CreateAsync(address);
        }
        return await addressesDao.UpdateAsync(address);
    }

    private async Task NotifySupportOfOrderAsync(
        Customers customer, Addresses? address, Orders order, string orderInfo)
    {
        var supportEmail = emailOptions.Value.SupportEmail;
        var subject = $"Order {order.Id}";
        var nl = Environment.NewLine;

        var body = new StringBuilder();
        body.Append("Customer: ").Append(customer.Name).Append(nl);
        body.Append("Address: ").Append(address is not null ? FormatAddress(address) : "N/A").Append(nl);
        body.Append(nl).Append("Order info:").Append(nl);
        body.Append("Order #").Append(order.Id)
            .Append(" - Status: ").Append(order.Status)
            .Append(" - Total: $").Append(order.Total)
            .Append(nl);
        body.Append(orderInfo);

        try
        {
            await emailDeliveryService.SendPlainTextEmailAsync([supportEmail], subject, body.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send order notification email for order {OrderId}", order.Id);
        }
    }

    private static string FormatAddress(Addresses address)
    {
        var sb = new StringBuilder();
        sb.Append(address.Line1);
        if (!string.IsNullOrWhiteSpace(address.Line2))
        {
            sb.Append(", ").Append(address.Line2);
        }
        sb.Append(", ").Append(address.City);
        if (!string.IsNullOrWhiteSpace(address.State))
        {
            sb.Append(", ").Append(address.State);
        }
        sb.Append(' ').Append(address.PostalCode).Append(", ").Append(address.Country);
        return sb.ToString();
    }

    private static DateTime Now() => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

    private static string FirstNonBlank(string? preferred, string fallback) =>
        !string.IsNullOrWhiteSpace(preferred) ? preferred : fallback;
}
