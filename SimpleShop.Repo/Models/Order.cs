using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SimpleShop.Repo.Models;

[Table("Order")]
public class Order
{
    [Key, Column("OrderID")] public int OrderId { get; set; }
    [Column("AccountID")] public int AccountId { get; set; }
    [Column("OrderDate")] public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    [Column("TotalAmount", TypeName = "numeric(18,2)")] public decimal TotalAmount { get; set; }
    [MaxLength(50), Column("Status")] public string Status { get; set; } = "Pending";

    public Account? Account { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

[Table("OrderItem")]
public class OrderItem
{
    [Key, Column("OrderItemID")] public int OrderItemId { get; set; }
    [Column("OrderID")] public int OrderId { get; set; }
    [Column("ProductID")] public int ProductId { get; set; }
    [Range(1, int.MaxValue), Column("Quantity")] public int Quantity { get; set; }
    [Column("UnitPrice", TypeName = "numeric(18,2)")] public decimal UnitPrice { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
