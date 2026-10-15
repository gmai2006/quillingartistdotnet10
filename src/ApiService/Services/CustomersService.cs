using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class CustomersService(CustomersDao dao, ILogger<CustomersService> logger)
{
    public async Task<Customers?> FindAsync(int id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(Customers) - exited");
        return result;
    }

    public Task<int> DeleteAsync(int id) => dao.DeleteAsync(id);

    /// <summary>Create Customers.</summary>
    public async Task<Customers> CreateAsync(Customers bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(Customers) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(Customers) - exited");
        return result;
    }

    /// <summary>Update a Customers.</summary>
    public async Task<Customers> UpdateAsync(Customers bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(Customers) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(Customers) - exited");
        return result;
    }

    public Task<List<Customers>> SelectAllAsync(int page, int pageSize, CustomersFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(CustomersFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, CustomersFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "Email, Name, CreatedAt",
            rows,
            r => [r.Email, r.Name, r.CreatedAt]);
    }
}
