using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record AddressesFilter(
    int? CustomerId = null,
    string? Line1 = null,
    string? Line2 = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null);

public class AddressesDao(AppDbContext db)
{
    public Task<Addresses?> FindAsync(int id) => db.Addresses.FindAsync(id).AsTask();

    public Task<int> NextIdAsync() => db.NextIdAsync("addresses_id_seq");

    public async Task<int> DeleteAsync(int id)
    {
        await db.Addresses.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<Addresses> CreateAsync(Addresses e)
    {
        db.Addresses.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Addresses> UpdateAsync(Addresses e)
    {
        db.Addresses.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<Addresses>> SelectAllAsync(int page, int pageSize, AddressesFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(AddressesFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<Addresses> BuildDynamicQuery(AddressesFilter f)
    {
        var q = db.Addresses.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(f.Line1))
        {
            var t = Paging.Term(f.Line1);
            q = q.Where(e => e.Line1.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Line2))
        {
            var t = Paging.Term(f.Line2);
            q = q.Where(e => e.Line2!.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.City))
        {
            var t = Paging.Term(f.City);
            q = q.Where(e => e.City.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.State))
        {
            var t = Paging.Term(f.State);
            q = q.Where(e => e.State!.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.PostalCode))
        {
            var t = Paging.Term(f.PostalCode);
            q = q.Where(e => e.PostalCode.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Country))
        {
            var t = Paging.Term(f.Country);
            q = q.Where(e => e.Country.ToLower().Contains(t));
        }

        if (f.CustomerId is not null)
        {
            q = q.Where(e => e.CustomerId == f.CustomerId);
        }

        return q;
    }
}
