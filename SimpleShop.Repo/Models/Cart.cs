using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SimpleShop.Repo.Models;

[Table("Cart")]
public class Cart
{
    [Key, Column("CartID")] public int CartId { get; set; }
    [Column("AccountID")] public int AccountId { get; set; }
    [Column("CreatedDate")] public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}

[Table("CartItem")]
public class CartItem
{
    [Key, Column("CartItemID")] public int CartItemId { get; set; }
    [Column("CartID")] public int CartId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Range(1, int.MaxValue), Column("Quantity")] public int Quantity { get; set; }

    public Cart? Cart { get; set; }
    public Product? Product { get; set; }
}
