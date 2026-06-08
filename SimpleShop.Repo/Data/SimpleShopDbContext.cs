using Microsoft.EntityFrameworkCore;
using SimpleShop.Repo.Models;
namespace SimpleShop.Repo.Data;
public class SimpleShopDbContext(DbContextOptions<SimpleShopDbContext> options):DbContext(options){
 public DbSet<Category> Categories=>Set<Category>(); public DbSet<Product> Products=>Set<Product>();
 protected override void OnModelCreating(ModelBuilder b){
  b.Entity<Category>().HasIndex(x=>x.CategoryName);
  b.Entity<Product>().HasOne(x=>x.Category).WithMany(x=>x.Products).HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
 }
}
