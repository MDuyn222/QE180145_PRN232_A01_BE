using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Data;
using SimpleShop.Repo.Models;

namespace SimpleShop.Repo.Repositories;

public class AccountRepository(SimpleShopDbContext dbContext) : IAccountRepository
{
    public Task<Account?> GetByEmailAsync(string email) =>
        dbContext.Accounts.FirstOrDefaultAsync(a =>
            a.Email.ToLower() == email.ToLower() && a.IsActive);

    public Task<Account?> GetByIdAsync(int id) =>
        dbContext.Accounts.FirstOrDefaultAsync(a => a.AccountId == id && a.IsActive);

    public async Task<Account> AddAsync(Account account)
    {
        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();
        return account;
    }

    public Task<bool> EmailExistsAsync(string email) =>
        dbContext.Accounts.AnyAsync(a => a.Email.ToLower() == email.ToLower());
}
