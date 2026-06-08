using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SimpleShop.Repo.Models;
[Table("Category")]
public class Category {
 [Key, Column("CategoryID")] public int CategoryId {get;set;}
 [Required, MaxLength(100), Column("CategoryName")] public string CategoryName {get;set;}=string.Empty;
 [Required, MaxLength(250), Column("CategoryDescription")] public string CategoryDescription {get;set;}=string.Empty;
 [Column("IsActive")] public bool IsActive {get;set;}=true;
 public ICollection<Product> Products {get;set;}=new List<Product>();
}
