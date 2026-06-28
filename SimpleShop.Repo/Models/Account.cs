using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SimpleShop.Repo.Models;

[Table("Account")]
public class Account
{
    [Key, Column("AccountID")] public int AccountId { get; set; }
    [Required, MaxLength(100), Column("FullName")] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(200), Column("Email")] public string Email { get; set; } = string.Empty;
    [Required, Column("PasswordHash")] public string PasswordHash { get; set; } = string.Empty;
    [Column("CreatedDate", TypeName = "timestamp without time zone")] public DateTime CreatedDate { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    [Column("IsActive")] public bool IsActive { get; set; } = true;

    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
