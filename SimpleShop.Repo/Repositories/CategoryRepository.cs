using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Data;
using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public class CategoryRepository(SimpleShopDbContext dbContext) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(bool includeInactive = false) =>
        dbContext.Categories
            .Where(category => includeInactive || category.IsActive)
            .OrderBy(category => category.CategoryName)
            .AsNoTracking()
            .ToListAsync();

    public Task<Category?> GetByIdAsync(int id, bool includeInactive = true) =>
        dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category =>
                category.CategoryId == id && (includeInactive || category.IsActive));

    public Task<List<Category>> SearchAsync(string name) =>
        dbContext.Categories
            .Where(category => EF.Functions.ILike(category.CategoryName, $"%{name.Trim()}%"))
            .OrderBy(category => category.CategoryName)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Category> AddAsync(Category category)
    {
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        dbContext.Categories.Update(category);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Category category)
    {
        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync();
    }

    public Task<bool> HasProductsAsync(int id) =>
        dbContext.Products.AnyAsync(product => product.CategoryId == id);
}
