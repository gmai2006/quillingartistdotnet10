using ApiService.Dao;
using ApiService.Entities;
using ApiService.Utils;

namespace ApiService.Services;

public class ProductsService(ProductsDao dao, ILogger<ProductsService> logger)
{
    public async Task<Products?> FindAsync(int id)
    {
        var result = await dao.FindAsync(id);
        logger.LogInformation("find(Products) - exited");
        return result;
    }

    public Task<int> DeleteAsync(int id) => dao.DeleteAsync(id);

    /// <summary>Create Products.</summary>
    public async Task<Products> CreateAsync(Products bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("create(Products) - entered");
        var result = await dao.CreateAsync(bean);
        logger.LogInformation("create(Products) - exited");
        return result;
    }

    /// <summary>Update a Products.</summary>
    public async Task<Products> UpdateAsync(Products bean)
    {
        ArgumentNullException.ThrowIfNull(bean);
        logger.LogInformation("update(Products) - entered");
        var result = await dao.UpdateAsync(bean);
        logger.LogInformation("update(Products) - exited");
        return result;
    }

    public async Task UpdateStockNumbersAsync(int productId, int quantity)
    {
        var product = await dao.FindAsync(productId)
            ?? throw new InvalidOperationException($"Product {productId} was not found.");
        product.Available -= quantity;
        await UpdateAsync(product);
    }

    public Task<List<Products>> SelectAllAsync(int page, int pageSize, ProductsFilter filter) =>
        dao.SelectAllAsync(page, pageSize, filter);

    public Task<long> CountAllAsync(ProductsFilter filter) => dao.CountAllAsync(filter);

    public async Task<string> ExportToCsvAsync(int page, int pageSize, ProductsFilter filter)
    {
        var rows = await dao.SelectAllAsync(page, pageSize, filter);
        return CsvBuilder.Build(
            "Name, Description, Price, Stock, Available, ImageUrl, CreatedAt",
            rows,
            r => [r.Name, r.Description, r.Price, r.Stock, r.Available, r.ImageUrl, r.CreatedAt]);
    }
}
