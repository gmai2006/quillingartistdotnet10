using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record ProductsQuery(
    int Page = 0,
    int PageSize = 25,
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? Stock = null,
    int? Available = null,
    string? ImageUrl = null,
    string? CreatedAt = null);

public static class ProductsEndpoints
{
    private const int MaxCatalogPageSize = 200;

    public static RouteGroupBuilder MapProductsEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/products").WithTags("Products");

        group.MapGet("", () => Results.Text(typeof(ProductsEndpoints).FullName));

        // Idempotent method. Update existing Products.
        group.MapPost("", (Products obj, ProductsService service) => service.UpdateAsync(obj));

        // Create a new Products.
        group.MapPut("", (Products obj, ProductsService service) => service.CreateAsync(obj));

        group.MapGet("catalog", async ([AsParameters] ProductsQuery query, ProductsService service) =>
        {
            var pageSize = Math.Clamp(query.PageSize, 1, MaxCatalogPageSize);
            var filter = ToFilter(query);
            var rows = await service.SelectAllAsync(query.Page, pageSize, filter);
            var totalCount = await service.CountAllAsync(filter);
            return Results.Ok(PageResponse<Products>.Of(rows, totalCount, query.Page, pageSize));
        });

        group.MapGet("selectAll", ([AsParameters] ProductsQuery query, AuthContext auth, ProductsService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<Products>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] ProductsQuery query, AuthContext auth, ProductsService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "products.csv");
            }));

        return group;
    }

    private static ProductsFilter ToFilter(ProductsQuery q) => new(
        q.Name,
        q.Description,
        q.Price,
        q.Stock,
        q.Available,
        q.ImageUrl,
        JsonHelper.ToLocalTimestamp(JsonHelper.ParsePerformed(q.CreatedAt, false)));
}
