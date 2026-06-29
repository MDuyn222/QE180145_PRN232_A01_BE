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

public interface IAccountRepository
{
    Task<Account?> GetByEmailAsync(string email);
    Task<Account?> GetByIdAsync(int id);
    Task<Account> AddAsync(Account account);
    Task<bool> EmailExistsAsync(string email);
}

public interface ICartRepository
{
    Task<Cart?> GetByAccountIdAsync(int accountId);
    Task<Cart> GetOrCreateCartAsync(int accountId);
    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task AddCartItemAsync(CartItem item);
    Task UpdateCartItemAsync(CartItem item);
    Task RemoveCartItemAsync(CartItem item);
    Task ClearCartAsync(int cartId);
}

public interface IOrderRepository
{
    Task<List<Order>> GetByAccountIdAsync(int accountId);
    Task<Order?> GetByIdAsync(int orderId, int accountId);
    Task<Order> AddAndClearCartAsync(Order order, int cartId);
}
