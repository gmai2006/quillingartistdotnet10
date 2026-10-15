using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record AddressesQuery(
    int Page = 0,
    int PageSize = 25,
    int? CustomerId = null,
    string? Line1 = null,
    string? Line2 = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null);

public static class AddressesEndpoints
{
    public static RouteGroupBuilder MapAddressesEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/addresses").WithTags("Addresses");

        group.MapGet("", () => Results.Text(typeof(AddressesEndpoints).FullName));

        // Idempotent method. Update existing Addresses.
        group.MapPost("", (Addresses obj, AddressesService service) => service.UpdateAsync(obj));

        // Create a new Addresses.
        group.MapPut("", (Addresses obj, AddressesService service) => service.CreateAsync(obj));

        group.MapGet("selectAll", ([AsParameters] AddressesQuery query, AuthContext auth, AddressesService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<Addresses>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] AddressesQuery query, AuthContext auth, AddressesService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "addresses.csv");
            }));

        return group;
    }

    private static AddressesFilter ToFilter(AddressesQuery q) => new(
        q.CustomerId, q.Line1, q.Line2, q.City, q.State, q.PostalCode, q.Country);
}
