using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Data;
using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public class OrderRepository(SimpleShopDbContext dbContext) : IOrderRepository
{
    public Task<List<Order>> GetByAccountIdAsync(int accountId) =>
        dbContext.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.AccountId == accountId)
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking()
            .ToListAsync();

    public Task<Order?> GetByIdAsync(int orderId, int accountId) =>
        dbContext.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OrderId == orderId && o.AccountId == accountId);

    public async Task<Order> AddAndClearCartAsync(Order order, int cartId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Orders.Add(order);

        var cartItems = await dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();

        dbContext.CartItems.RemoveRange(cartItems);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return await GetByIdAsync(order.OrderId, order.AccountId) ?? order;
    }
}
