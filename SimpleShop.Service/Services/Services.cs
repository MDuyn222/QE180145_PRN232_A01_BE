using SimpleShop.Repo.Models;
using SimpleShop.Repo.Repositories;
using SimpleShop.Service.DTOs;

namespace SimpleShop.Service.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(bool includeInactive);
    Task<CategoryDto?> GetAsync(int id, bool includeInactive = false);
    Task<List<CategoryDto>> SearchAsync(string name);
    Task<CategoryDto> CreateAsync(CategoryRequest request);
    Task<bool> UpdateAsync(int id, CategoryRequest request);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(bool includeInactive);
    Task<ProductDto?> GetAsync(int id, bool includeInactive = false);
    Task<List<ProductDto>> GetByCategoryAsync(int categoryId);
    Task<List<ProductDto>> SearchAsync(string? name, decimal? minPrice, decimal? maxPrice, int? categoryId);
    Task<ProductDto> CreateAsync(ProductRequest request);
    Task<bool> UpdateAsync(int id, ProductRequest request);
    Task<bool> SoftDeleteAsync(int id);
}

internal static class DtoMapper
{
    public static CategoryDto ToDto(Category category) =>
        new(category.CategoryId, category.CategoryName, category.CategoryDescription, category.IsActive);

    public static ProductDto ToDto(Product product) =>
        new(
            product.ProductId,
            product.ProductName,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.ImageUrl,
            product.CategoryId,
            product.Category?.CategoryName,
            product.IsActive,
            product.CreatedDate,
            product.ModifiedDate);
}

public class CategoryService(ICategoryRepository repository) : ICategoryService
{
    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive) =>
        (await repository.GetAllAsync(includeInactive)).Select(DtoMapper.ToDto).ToList();

    public async Task<CategoryDto?> GetAsync(int id, bool includeInactive = false)
    {
        var category = await repository.GetByIdAsync(id, includeInactive);
        return category is null ? null : DtoMapper.ToDto(category);
    }

    public async Task<List<CategoryDto>> SearchAsync(string name) =>
        (await repository.SearchAsync(name)).Select(DtoMapper.ToDto).ToList();

    public async Task<CategoryDto> CreateAsync(CategoryRequest request)
    {
        var category = await repository.AddAsync(new Category
        {
            CategoryName = request.CategoryName.Trim(),
            CategoryDescription = request.CategoryDescription.Trim(),
            IsActive = request.IsActive
        });

        return DtoMapper.ToDto(category);
    }

    public async Task<bool> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await repository.GetByIdAsync(id, true);
        if (category is null)
        {
            return false;
        }

        category.CategoryName = request.CategoryName.Trim();
        category.CategoryDescription = request.CategoryDescription.Trim();
        category.IsActive = request.IsActive;
        await repository.UpdateAsync(category);
        return true;
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        var category = await repository.GetByIdAsync(id, true);
        if (category is null)
        {
            return (false, "Category not found.");
        }

        if (await repository.HasProductsAsync(id))
        {
            return (false, "Cannot delete this category because products are linked to it.");
        }

        await repository.DeleteAsync(category);
        return (true, null);
    }
}

public class ProductService(
    IProductRepository repository,
    ICategoryRepository categoryRepository) : IProductService
{
    public async Task<List<ProductDto>> GetAllAsync(bool includeInactive) =>
        (await repository.GetAllAsync(includeInactive)).Select(DtoMapper.ToDto).ToList();

    public async Task<ProductDto?> GetAsync(int id, bool includeInactive = false)
    {
        var product = await repository.GetByIdAsync(id, includeInactive);
        return product is null ? null : DtoMapper.ToDto(product);
    }

    public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId) =>
        (await repository.GetByCategoryAsync(categoryId)).Select(DtoMapper.ToDto).ToList();

    public async Task<List<ProductDto>> SearchAsync(
        string? name,
        decimal? minPrice,
        decimal? maxPrice,
        int? categoryId)
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
        {
            throw new ArgumentException("Minimum price cannot be greater than maximum price.");
        }

        return (await repository.SearchAsync(name, minPrice, maxPrice, categoryId))
            .Select(DtoMapper.ToDto)
            .ToList();
    }

    public async Task<ProductDto> CreateAsync(ProductRequest request)
    {
        if (await categoryRepository.GetByIdAsync(request.CategoryId, true) is null)
        {
            throw new ArgumentException("Category does not exist.");
        }

        var product = await repository.AddAsync(new Product
        {
            ProductName = request.ProductName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            CategoryId = request.CategoryId,
            IsActive = request.IsActive,
            CreatedDate = DateTime.UtcNow
        });

        return DtoMapper.ToDto(product);
    }

    public async Task<bool> UpdateAsync(int id, ProductRequest request)
    {
        var product = await repository.GetByIdAsync(id, true);
        if (product is null)
        {
            return false;
        }

        if (await categoryRepository.GetByIdAsync(request.CategoryId, true) is null)
        {
            throw new ArgumentException("Category does not exist.");
        }

        product.ProductName = request.ProductName.Trim();
        product.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;
        product.ModifiedDate = DateTime.UtcNow;
        await repository.UpdateAsync(product);
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var product = await repository.GetByIdAsync(id, true);
        if (product is null)
        {
            return false;
        }

        product.IsActive = false;
        product.ModifiedDate = DateTime.UtcNow;
        await repository.UpdateAsync(product);
        return true;
    }
}
