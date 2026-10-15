using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record CustomersFilter(string? Email = null, string? Name = null, DateTime? CreatedAt = null);

public class CustomersDao(AppDbContext db)
{
    public Task<Customers?> FindAsync(int id) => db.Customers.FindAsync(id).AsTask();

    public Task<int> NextIdAsync() => db.NextIdAsync("customers_id_seq");

    public Task<Customers?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult<Customers?>(null);
        }

        var normalized = Paging.Term(email);
        return db.Customers.FirstOrDefaultAsync(e => e.Email.ToLower() == normalized);
    }

    public async Task<int> DeleteAsync(int id)
    {
        await db.Customers.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<Customers> CreateAsync(Customers e)
    {
        db.Customers.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Customers> UpdateAsync(Customers e)
    {
        db.Customers.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<Customers>> SelectAllAsync(int page, int pageSize, CustomersFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(CustomersFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<Customers> BuildDynamicQuery(CustomersFilter f)
    {
        var q = db.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(f.Email))
        {
            var t = Paging.Term(f.Email);
            q = q.Where(e => e.Email.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Name))
        {
            var t = Paging.Term(f.Name);
            q = q.Where(e => e.Name.ToLower().Contains(t));
        }

        if (f.CreatedAt is not null)
        {
            q = q.Where(e => e.CreatedAt == f.CreatedAt);
        }

        return q;
    }
}
