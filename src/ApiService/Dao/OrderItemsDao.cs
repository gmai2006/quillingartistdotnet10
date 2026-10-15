using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record OrderItemsFilter(
    int? OrderId = null,
    int? ProductId = null,
    int? Quantity = null,
    decimal? UnitPrice = null);

public class OrderItemsDao(AppDbContext db)
{
    public Task<OrderItems?> FindAsync(int id) => db.OrderItems.FindAsync(id).AsTask();

    public Task<int> NextIdAsync() => db.NextIdAsync("order_items_id_seq");

    public async Task<int> DeleteAsync(int id)
    {
        await db.OrderItems.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<OrderItems> CreateAsync(OrderItems e)
    {
        db.OrderItems.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<OrderItems> UpdateAsync(OrderItems e)
    {
        db.OrderItems.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<OrderItems>> SelectAllAsync(int page, int pageSize, OrderItemsFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(OrderItemsFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<OrderItems> BuildDynamicQuery(OrderItemsFilter f)
    {
        var q = db.OrderItems.AsNoTracking();

        if (f.OrderId is not null)
        {
            q = q.Where(e => e.OrderId == f.OrderId);
        }

        if (f.ProductId is not null)
        {
            q = q.Where(e => e.ProductId == f.ProductId);
        }

        if (f.Quantity is not null)
        {
            q = q.Where(e => e.Quantity == f.Quantity);
        }

        if (f.UnitPrice is not null)
        {
            q = q.Where(e => e.UnitPrice == f.UnitPrice);
        }

        return q;
    }
}
