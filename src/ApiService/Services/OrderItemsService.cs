using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class OrderItemsService(OrderItemsDao dao, ILogger<OrderItemsService> logger)
{
    public async Task<OrderItems?> FindAsync(int id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(OrderItems) - exited");
        return result;
    }

    public Task<int> DeleteAsync(int id) => dao.DeleteAsync(id);

    /// <summary>Create OrderItems.</summary>
    public async Task<OrderItems> CreateAsync(OrderItems bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(OrderItems) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(OrderItems) - exited");
        return result;
    }

    /// <summary>Update a OrderItems.</summary>
    public async Task<OrderItems> UpdateAsync(OrderItems bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(OrderItems) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(OrderItems) - exited");
        return result;
    }

    public Task<List<OrderItems>> SelectAllAsync(int page, int pageSize, OrderItemsFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(OrderItemsFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, OrderItemsFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "OrderId, ProductId, Quantity, UnitPrice",
            rows,
            r => [r.OrderId, r.ProductId, r.Quantity, r.UnitPrice]);
    }
}
