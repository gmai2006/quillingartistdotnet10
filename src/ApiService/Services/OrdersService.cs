using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class OrdersService(OrdersDao dao, ILogger<OrdersService> logger)
{
    public async Task<Orders?> FindAsync(int id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(Orders) - exited");
        return result;
    }

    public Task<int> DeleteAsync(int id) => dao.DeleteAsync(id);

    /// <summary>Create Orders.</summary>
    public async Task<Orders> CreateAsync(Orders bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(Orders) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(Orders) - exited");
        return result;
    }

    /// <summary>Update a Orders.</summary>
    public async Task<Orders> UpdateAsync(Orders bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(Orders) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(Orders) - exited");
        return result;
    }

    public Task<List<Orders>> SelectAllAsync(int page, int pageSize, OrdersFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(OrdersFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, OrdersFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "CustomerId, ShippingAddressId, Status, Total, CreatedAt",
            rows,
            r => [r.CustomerId, r.ShippingAddressId, r.Status, r.Total, r.CreatedAt]);
    }
}
