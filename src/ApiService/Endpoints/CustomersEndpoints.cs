using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record CustomersQuery(
    int Page = 0,
    int PageSize = 25,
    string? Email = null,
    string? Name = null,
    string? CreatedAt = null);

public static class CustomersEndpoints
{
    public static RouteGroupBuilder MapCustomersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/customers").WithTags("Customers");

        group.MapGet("", () => Results.Text(typeof(CustomersEndpoints).FullName));

        // Idempotent method. Update existing Customers.
        group.MapPost("", (Customers obj, CustomersService service) => service.UpdateAsync(obj));

        // Create a new Customers.
        group.MapPut("", (Customers obj, CustomersService service) => service.CreateAsync(obj));

        group.MapGet("selectAll", ([AsParameters] CustomersQuery query, AuthContext auth, CustomersService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<Customers>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] CustomersQuery query, AuthContext auth, CustomersService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "customers.csv");
            }));

        return group;
    }

    private static CustomersFilter ToFilter(CustomersQuery q) => new(
        q.Email,
        q.Name,
        JsonHelper.ToLocalTimestamp(JsonHelper.ParsePerformed(q.CreatedAt, false)));
}
