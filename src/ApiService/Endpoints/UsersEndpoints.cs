using ApiService.Dao;
using ApiService.Dtos;
using ApiService.Entities;
using ApiService.Security;
using ApiService.Services;
using ApiService.Utils;
using static ApiService.Endpoints.EndpointHelpers;

namespace ApiService.Endpoints;

public record UsersQuery(
    int Page = 0,
    int PageSize = 25,
    string? Username = null,
    string? Email = null,
    string? FullName = null,
    string? Role = null,
    bool? MfaEnabled = null,
    bool? IsActive = null,
    string? CreatedAt = null,
    string? LastLoginAt = null,
    string? MfaSecret = null,
    string? MfaEnrolledAt = null);

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/users").WithTags("Users");

        group.MapGet("", () => Results.Text(typeof(UsersEndpoints).FullName));

        // Idempotent method. Update existing Users.
        group.MapPost("", (Users obj, UsersService service) => service.UpdateAsync(obj));

        // Create a new Users.
        group.MapPut("", (Users obj, UsersService service) => service.CreateAsync(obj));

        group.MapGet("selectAll", ([AsParameters] UsersQuery query, AuthContext auth, UsersService service) =>
            Authorized(auth, async () =>
            {
                var filter = ToFilter(query);
                var rows = await service.SelectAllAsync(query.Page, query.PageSize, filter);
                var totalCount = await service.CountAllAsync(filter);
                return Results.Ok(PageResponse<Users>.Of(rows, totalCount, query.Page, query.PageSize));
            }));

        group.MapGet("export", ([AsParameters] UsersQuery query, AuthContext auth, UsersService service) =>
            Authorized(auth, async () =>
            {
                var csv = await service.ExportToCsvAsync(query.Page, query.PageSize, ToFilter(query));
                return Csv(csv, "users.csv");
            }));

        return group;
    }

    private static UsersFilter ToFilter(UsersQuery q) => new(
        q.Username,
        q.Email,
        q.FullName,
        q.Role,
        q.MfaEnabled,
        q.IsActive,
        JsonHelper.ToUtc(JsonHelper.ParsePerformed(q.CreatedAt, false)),
        JsonHelper.ToUtc(JsonHelper.ParsePerformed(q.LastLoginAt, false)),
        q.MfaSecret,
        JsonHelper.ToUtc(JsonHelper.ParsePerformed(q.MfaEnrolledAt, false)));
}
