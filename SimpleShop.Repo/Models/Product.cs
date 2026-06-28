using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SimpleShop.Repo.Models;
[Table("Product")]
public class Product {
 [Key, Column("ProductID")] public int ProductId {get;set;}
 [Required, MaxLength(200), Column("ProductName")] public string ProductName {get;set;}=string.Empty;
 [Column("Description")] public string? Description {get;set;}
 [Range(0,double.MaxValue), Column("Price",TypeName="numeric(18,2)")] public decimal Price {get;set;}
 [Range(0,int.MaxValue), Column("StockQuantity")] public int StockQuantity {get;set;}
 [MaxLength(500), Column("ImageUrl")] public string? ImageUrl {get;set;}
 [Column("CategoryID")] public int CategoryId {get;set;}
 [Column("AccountID")] public int? AccountId {get;set;}
 [Column("IsActive")] public bool IsActive {get;set;}=true;
 [Column("CreatedDate")] public DateTime CreatedDate {get;set;}=DateTime.Now;
 [Column("ModifiedDate")] public DateTime? ModifiedDate {get;set;}
 public Category? Category {get;set;}
 public Account? Account {get;set;}
}
