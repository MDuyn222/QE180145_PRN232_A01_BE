using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Data;
using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public class ProductRepository(SimpleShopDbContext dbContext) : IProductRepository
{
    public Task<List<Product>> GetAllAsync(bool includeInactive = false) =>
        dbContext.Products
            .Include(product => product.Category)
            .Where(product => includeInactive || (product.IsActive && product.Category!.IsActive))
            .OrderByDescending(product => product.CreatedDate)
            .AsNoTracking()
            .ToListAsync();

    public Task<Product?> GetByIdAsync(int id, bool includeInactive = true) =>
        dbContext.Products
            .Include(product => product.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(product =>
                product.ProductId == id
                && (includeInactive || (product.IsActive && product.Category!.IsActive)));

    public Task<List<Product>> GetByCategoryAsync(int categoryId) =>
        dbContext.Products
            .Include(product => product.Category)
            .Where(product =>
                product.CategoryId == categoryId
                && product.IsActive
                && product.Category!.IsActive)
            .OrderBy(product => product.ProductName)
            .AsNoTracking()
            .ToListAsync();

    public Task<List<Product>> SearchAsync(
        string? name,
        decimal? minPrice,
        decimal? maxPrice,
        int? categoryId)
    {
        var query = dbContext.Products
            .Include(product => product.Category)
            .Where(product => product.IsActive && product.Category!.IsActive);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(product =>
                EF.Functions.ILike(product.ProductName, $"%{name.Trim()}%"));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(product => product.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(product => product.Price <= maxPrice.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == categoryId.Value);
        }

        return query
            .OrderBy(product => product.ProductName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        return await GetByIdAsync(product.ProductId)
            ?? product;
    }

    public async Task UpdateAsync(Product product)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync();
    }
}
