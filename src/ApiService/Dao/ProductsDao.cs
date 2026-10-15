using ApiService.Data;
using ApiService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Dao;

public record ProductsFilter(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? Stock = null,
    int? Available = null,
    string? ImageUrl = null,
    DateTime? CreatedAt = null);

public class ProductsDao(AppDbContext db)
{
    public Task<Products?> FindAsync(int id) => db.Products.FindAsync(id).AsTask();

    public async Task<int> DeleteAsync(int id)
    {
        await db.Products.Where(e => e.Id == id).ExecuteDeleteAsync();
        return id;
    }

    public async Task<Products> CreateAsync(Products e)
    {
        db.Products.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Products> UpdateAsync(Products e)
    {
        db.Products.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public Task<List<Products>> SelectAllAsync(int page, int pageSize, ProductsFilter filter) =>
        BuildDynamicQuery(filter).OrderBy(e => e.Id).ApplyPagination(page, pageSize).ToListAsync();

    public Task<long> CountAllAsync(ProductsFilter filter) => BuildDynamicQuery(filter).LongCountAsync();

    private IQueryable<Products> BuildDynamicQuery(ProductsFilter f)
    {
        var q = db.Products.AsNoTracking().Where(e => e.Stock > 0);

        if (!string.IsNullOrWhiteSpace(f.Name))
        {
            var t = Paging.Term(f.Name);
            q = q.Where(e => e.Name.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.Description))
        {
            var t = Paging.Term(f.Description);
            q = q.Where(e => e.Description!.ToLower().Contains(t));
        }

        if (!string.IsNullOrWhiteSpace(f.ImageUrl))
        {
            var t = Paging.Term(f.ImageUrl);
            q = q.Where(e => e.ImageUrl!.ToLower().Contains(t));
        }

        if (f.Price is not null)
        {
            q = q.Where(e => e.Price == f.Price);
        }

        if (f.Stock is not null)
        {
            q = q.Where(e => e.Stock == f.Stock);
        }

        if (f.Available is not null)
        {
            q = q.Where(e => e.Available == f.Available);
        }

        if (f.CreatedAt is not null)
        {
            q = q.Where(e => e.CreatedAt == f.CreatedAt);
        }

        return q;
    }
}
