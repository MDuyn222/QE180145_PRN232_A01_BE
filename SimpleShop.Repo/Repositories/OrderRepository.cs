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

    public async Task<Order> AddAsync(Order order)
    {
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
        return order;
    }
}
