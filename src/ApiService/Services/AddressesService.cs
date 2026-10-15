using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class AddressesService(AddressesDao dao, ILogger<AddressesService> logger)
{
    public async Task<Addresses?> FindAsync(int id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(Addresses) - exited");
        return result;
    }

    public Task<int> DeleteAsync(int id) => dao.DeleteAsync(id);

    /// <summary>Create Addresses.</summary>
    public async Task<Addresses> CreateAsync(Addresses bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(Addresses) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(Addresses) - exited");
        return result;
    }

    /// <summary>Update a Addresses.</summary>
    public async Task<Addresses> UpdateAsync(Addresses bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(Addresses) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(Addresses) - exited");
        return result;
    }

    public Task<List<Addresses>> SelectAllAsync(int page, int pageSize, AddressesFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(AddressesFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, AddressesFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "CustomerId, Line1, Line2, City, State, PostalCode, Country",
            rows,
            r => [r.CustomerId, r.Line1, r.Line2, r.City, r.State, r.PostalCode, r.Country]);
    }
}
