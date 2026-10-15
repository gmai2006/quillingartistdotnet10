using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class UsersService(UsersDao dao, ILogger<UsersService> logger)
{
    public async Task<Users?> FindAsync(Guid id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(Users) - exited");
        return result;
    }

    public Task<Guid> DeleteAsync(Guid id) => dao.DeleteAsync(id);

    /// <summary>Create Users.</summary>
    public async Task<Users> CreateAsync(Users bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(Users) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(Users) - exited");
        return result;
    }

    /// <summary>Update a Users.</summary>
    public async Task<Users> UpdateAsync(Users bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(Users) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(Users) - exited");
        return result;
    }

    public Task<List<Users>> SelectAllAsync(int page, int pageSize, UsersFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(UsersFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, UsersFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "Username, Email, FullName, Role, MfaEnabled, IsActive, CreatedAt, LastLoginAt, MfaSecret, MfaEnrolledAt",
            rows,
            r => [r.Username, r.Email, r.FullName, r.Role, r.MfaEnabled, r.IsActive, r.CreatedAt, r.LastLoginAt, r.MfaSecret, r.MfaEnrolledAt]);
    }
}
