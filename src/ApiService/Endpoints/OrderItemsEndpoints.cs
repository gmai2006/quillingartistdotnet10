using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record OrderItemsQuery(
    int Page = 0,
    int PageSize = 25,
    int? OrderId = null,
    int? ProductId = null,
    int? Quantity = null,
    decimal? UnitPrice = null);

public static class OrderItemsEndpoints
{
    public static RouteGroupBuilder MapOrderItemsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/orderitems").WithTags("OrderItems");

        group.MapGet("", () => Results.Text(typeof(OrderItemsEndpoints).FullName));

        // Idempotent method. Update existing OrderItems.
        group.MapPost("", (OrderItems obj, OrderItemsService service) => service.UpdateAsync(obj));

        // Create a new OrderItems.
        group.MapPut("", (OrderItems obj, OrderItemsService service) => service.CreateAsync(obj));

        group.MapGet("selectAll", ([AsParameters] OrderItemsQuery query, AuthContext auth, OrderItemsService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<OrderItems>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] OrderItemsQuery query, AuthContext auth, OrderItemsService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "orderitems.csv");
            }));

        return group;
    }

    private static OrderItemsFilter ToFilter(OrderItemsQuery q) => new(
        q.OrderId, q.ProductId, q.Quantity, q.UnitPrice);
}
