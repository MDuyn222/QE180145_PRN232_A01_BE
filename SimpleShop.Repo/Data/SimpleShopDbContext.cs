using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Models;
namespace SimpleShop.Repo.Data;

public class SimpleShopDbContext(DbContextOptions<SimpleShopDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Category>().HasIndex(x => x.CategoryName);
        b.Entity<Account>().HasIndex(x => x.Email).IsUnique();

        b.Entity<Product>()
            .HasOne(x => x.Category).WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Cart>()
            .HasOne(x => x.Account).WithMany(x => x.Carts)
            .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<CartItem>()
            .HasOne(x => x.Cart).WithMany(x => x.CartItems)
            .HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<CartItem>()
            .HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Order>()
            .HasOne(x => x.Account).WithMany(x => x.Orders)
            .HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<OrderItem>()
            .HasOne(x => x.Order).WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<OrderItem>()
            .HasOne(x => x.Product).WithMany()
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}
