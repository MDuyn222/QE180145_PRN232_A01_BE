using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Data;
using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public class CartRepository(SimpleShopDbContext dbContext) : ICartRepository
{
    public Task<Cart?> GetByAccountIdAsync(int accountId) =>
        dbContext.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.AccountId == accountId);

    public async Task<Cart> GetOrCreateCartAsync(int accountId)
    {
        var cart = await GetByAccountIdAsync(accountId);
        if (cart is not null) return cart;

        cart = new Cart { AccountId = accountId };
        dbContext.Carts.Add(cart);
        await dbContext.SaveChangesAsync();
        return cart;
    }

    public Task<CartItem?> GetCartItemAsync(int cartId, int productId) =>
        dbContext.CartItems.FirstOrDefaultAsync(ci =>
            ci.CartId == cartId && ci.ProductId == productId);

    public Task<CartItem?> GetCartItemByIdAsync(int cartItemId) =>
        dbContext.CartItems.Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

    public async Task AddCartItemAsync(CartItem item)
    {
        dbContext.CartItems.Add(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateCartItemAsync(CartItem item)
    {
        dbContext.CartItems.Update(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task RemoveCartItemAsync(CartItem item)
    {
        dbContext.CartItems.Remove(item);
        await dbContext.SaveChangesAsync();
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = dbContext.CartItems.Where(ci => ci.CartId == cartId);
        dbContext.CartItems.RemoveRange(items);
        await dbContext.SaveChangesAsync();
    }
}
