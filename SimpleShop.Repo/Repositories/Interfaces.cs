using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(bool includeInactive = false);
    Task<Category?> GetByIdAsync(int id, bool includeInactive = true);
    Task<List<Category>> SearchAsync(string name);
    Task<Category> AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task<bool> HasProductsAsync(int id);
}

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(bool includeInactive = false);
    Task<Product?> GetByIdAsync(int id, bool includeInactive = true);
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<List<Product>> SearchAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
}
