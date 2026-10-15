namespace ApiService.Dtos;

public class CartItemDto
{
    public int? ProductId { get; set; }
    public int? Quantity { get; set; }
}

public class CheckoutRequest
{
    public List<CartItemDto>? Items { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? SuccessUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public class CheckoutSessionResponse
{
    public string? SessionId { get; set; }
    public string? Url { get; set; }
}

public class CheckoutConfirmRequest
{
    public string? SessionId { get; set; }
}

public class CheckoutConfirmResponse
{
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? Status { get; set; }
    public decimal? Total { get; set; }
}
