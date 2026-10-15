using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record OrdersFilter(
    int? CustomerId = null,
    int? ShippingAddressId = null,
    string? Status = null,
    decimal? Total = null,
    DateTime? CreatedAt = null);

public class OrdersDao(AppDbContext db)
{
    public Task<Orders?> FindAsync(int id) => db.Orders.FindAsync(id).AsTask();

    public Task<int> NextIdAsync() => db.NextIdAsync("orders_id_seq");

    public async Task<int> DeleteAsync(int id)
    {
        await db.Orders.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<Orders> CreateAsync(Orders e)
    {
        db.Orders.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Orders> UpdateAsync(Orders e)
    {
        db.Orders.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<Orders>> SelectAllAsync(int page, int pageSize, OrdersFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(OrdersFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<Orders> BuildDynamicQuery(OrdersFilter f)
    {
        var q = db.Orders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(f.Status))
        {
            var t = Paging.Term(f.Status);
            q = q.Where(e => e.Status.ToLower().Contains(t));
        }

        if (f.CustomerId is not null)
        {
            q = q.Where(e => e.CustomerId == f.CustomerId);
        }

        if (f.ShippingAddressId is not null)
        {
            q = q.Where(e => e.ShippingAddressId == f.ShippingAddressId);
        }

        if (f.Total is not null)
        {
            q = q.Where(e => e.Total == f.Total);
        }

        if (f.CreatedAt is not null)
        {
            q = q.Where(e => e.CreatedAt == f.CreatedAt);
        }

        return q;
    }
}
