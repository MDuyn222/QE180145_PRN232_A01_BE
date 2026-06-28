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
    Task<ProductDto> CreateAsync(ProductRequest request, int? accountId);
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
            product.AccountId,
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

    public async Task<ProductDto> CreateAsync(ProductRequest request, int? accountId)
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
            AccountId = accountId,
            IsActive = request.IsActive,
            CreatedDate = DateTime.Now
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
        product.ModifiedDate = DateTime.Now;
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
        product.ModifiedDate = DateTime.Now;
        await repository.UpdateAsync(product);
        return true;
    }
}

public interface IAccountService
{
    Task<(AccountDto? Account, string? Error)> RegisterAsync(RegisterRequest request);
    Task<Account?> ValidateAsync(string email, string password);
}

public class AccountService(IAccountRepository repository) : IAccountService
{
    public async Task<(AccountDto? Account, string? Error)> RegisterAsync(RegisterRequest request)
    {
        if (await repository.EmailExistsAsync(request.Email.Trim()))
        {
            return (null, "Email already registered.");
        }

        var account = await repository.AddAsync(new Account
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        });

        return (new AccountDto(account.AccountId, account.FullName, account.Email), null);
    }

    public async Task<Account?> ValidateAsync(string email, string password)
    {
        var account = await repository.GetByEmailAsync(email.Trim());
        if (account is null)
        {
            return null;
        }

        return BCrypt.Net.BCrypt.Verify(password, account.PasswordHash) ? account : null;
    }
}

public interface ICartService
{
    Task<CartDto> GetCartAsync(int accountId);
    Task<(bool Success, string? Error)> AddToCartAsync(int accountId, AddToCartRequest request);
    Task<(bool Success, string? Error)> UpdateCartItemAsync(int accountId, int cartItemId, UpdateCartItemRequest request);
    Task<(bool Success, string? Error)> RemoveCartItemAsync(int accountId, int cartItemId);
}

public class CartService(ICartRepository cartRepository, IProductRepository productRepository) : ICartService
{
    public async Task<CartDto> GetCartAsync(int accountId)
    {
        var cart = await cartRepository.GetOrCreateCartAsync(accountId);
        return MapToDto(cart);
    }

    public async Task<(bool Success, string? Error)> AddToCartAsync(int accountId, AddToCartRequest request)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, false);
        if (product is null)
        {
            return (false, "Product not found or unavailable.");
        }

        var cart = await cartRepository.GetOrCreateCartAsync(accountId);
        var existing = await cartRepository.GetCartItemAsync(cart.CartId, request.ProductId);

        var newQty = (existing?.Quantity ?? 0) + request.Quantity;
        if (newQty > product.StockQuantity)
        {
            return (false, $"Only {product.StockQuantity} item(s) in stock.");
        }

        if (existing is not null)
        {
            existing.Quantity = newQty;
            await cartRepository.UpdateCartItemAsync(existing);
        }
        else
        {
            await cartRepository.AddCartItemAsync(new CartItem
            {
                CartId = cart.CartId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateCartItemAsync(int accountId, int cartItemId, UpdateCartItemRequest request)
    {
        var cart = await cartRepository.GetByAccountIdAsync(accountId);
        if (cart is null)
        {
            return (false, "Cart not found.");
        }

        var item = await cartRepository.GetCartItemByIdAsync(cartItemId);
        if (item is null || item.CartId != cart.CartId)
        {
            return (false, "Item not found.");
        }

        if (item.Product is null)
        {
            return (false, "Product not found or unavailable.");
        }

        if (request.Quantity > item.Product.StockQuantity)
        {
            return (false, $"Only {item.Product.StockQuantity} item(s) in stock.");
        }

        item.Quantity = request.Quantity;
        await cartRepository.UpdateCartItemAsync(item);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RemoveCartItemAsync(int accountId, int cartItemId)
    {
        var cart = await cartRepository.GetByAccountIdAsync(accountId);
        if (cart is null)
        {
            return (false, "Cart not found.");
        }

        var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
        if (item is null)
        {
            return (false, "Item not found.");
        }

        await cartRepository.RemoveCartItemAsync(item);
        return (true, null);
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.CartItems
            .Where(ci => ci.Product is not null)
            .Select(ci => new CartItemDto(
                ci.CartItemId,
                ci.ProductId,
                ci.Product!.ProductName,
                ci.Product.ImageUrl,
                ci.Product.Price,
                ci.Quantity,
                ci.Product.Price * ci.Quantity))
            .ToList();

        return new CartDto(cart.CartId, items, items.Sum(i => i.Subtotal));
    }
}

public interface IOrderService
{
    Task<(OrderDto? Order, string? Error)> CheckoutAsync(int accountId);
    Task<List<OrderDto>> GetOrdersAsync(int accountId);
    Task<OrderDto?> GetOrderAsync(int orderId, int accountId);
}

public class OrderService(ICartRepository cartRepository, IOrderRepository orderRepository) : IOrderService
{
    public async Task<(OrderDto? Order, string? Error)> CheckoutAsync(int accountId)
    {
        var cart = await cartRepository.GetByAccountIdAsync(accountId);
        if (cart is null || !cart.CartItems.Any())
        {
            return (null, "Cart is empty.");
        }

        var orderItems = new List<OrderItem>();

        foreach (var ci in cart.CartItems)
        {
            var product = ci.Product;

            if (product is null || !product.IsActive)
            {
                return (null, $"Product '{ci.ProductId}' is no longer available.");
            }

            if (ci.Quantity > product.StockQuantity)
            {
                return (null, $"Insufficient stock for '{product.ProductName}'.");
            }

            orderItems.Add(new OrderItem
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = product.Price
            });
        }

        foreach (var ci in cart.CartItems)
        {
            var product = ci.Product!;
            product.StockQuantity -= ci.Quantity;
            product.ModifiedDate = DateTime.Now;
        }

        var order = await orderRepository.AddAsync(new Order
        {
            AccountId = accountId,
            TotalAmount = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity),
            OrderItems = orderItems
        });

        await cartRepository.ClearCartAsync(cart.CartId);

        return (MapToDto(order), null);
    }

    public async Task<List<OrderDto>> GetOrdersAsync(int accountId) =>
        (await orderRepository.GetByAccountIdAsync(accountId)).Select(MapToDto).ToList();

    public async Task<OrderDto?> GetOrderAsync(int orderId, int accountId)
    {
        var order = await orderRepository.GetByIdAsync(orderId, accountId);
        return order is null ? null : MapToDto(order);
    }

    private static OrderDto MapToDto(Order order)
    {
        var items = order.OrderItems.Select(oi => new OrderItemDto(
            oi.OrderItemId,
            oi.ProductId,
            oi.Product?.ProductName ?? $"Product #{oi.ProductId}",
            oi.UnitPrice,
            oi.Quantity,
            oi.UnitPrice * oi.Quantity)).ToList();

        return new OrderDto(order.OrderId, order.OrderDate, order.TotalAmount, order.Status, items);
    }
}