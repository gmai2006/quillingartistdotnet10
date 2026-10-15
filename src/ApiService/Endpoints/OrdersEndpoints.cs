using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record OrdersQuery(
    int Page = 0,
    int PageSize = 25,
    int? CustomerId = null,
    int? ShippingAddressId = null,
    string? Status = null,
    decimal? Total = null,
    string? CreatedAt = null);

public static class OrdersEndpoints
{
    public static RouteGroupBuilder MapOrdersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/orders").WithTags("Orders");

        group.MapGet("", () => Results.Text(typeof(OrdersEndpoints).FullName));

        // Idempotent method. Update existing Orders.
        group.MapPost("", (Orders obj, OrdersService service) => service.UpdateAsync(obj));

        // Create a new Orders.
        group.MapPut("", (Orders obj, OrdersService service) => service.CreateAsync(obj));

        group.MapGet("selectAll", ([AsParameters] OrdersQuery query, AuthContext auth, OrdersService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<Orders>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] OrdersQuery query, AuthContext auth, OrdersService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "orders.csv");
            }));

        return group;
    }

    private static OrdersFilter ToFilter(OrdersQuery q) => new(
        q.CustomerId,
        q.ShippingAddressId,
        q.Status,
        q.Total,
        JsonHelper.ToLocalTimestamp(JsonHelper.ParsePerformed(q.CreatedAt, false)));
}
