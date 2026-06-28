using System.ComponentModel.DataAnnotations;
namespace SimpleShop.Service.DTOs;
public record CategoryDto(int CategoryId,string CategoryName,string CategoryDescription,bool IsActive);
public class CategoryRequest{[Required,MaxLength(100)]public string CategoryName{get;set;}=string.Empty;[Required,MaxLength(250)]public string CategoryDescription{get;set;}=string.Empty;public bool IsActive{get;set;}=true;}
public record ProductDto(int ProductId,string ProductName,string? Description,decimal Price,int StockQuantity,string? ImageUrl,int CategoryId,string? CategoryName,bool IsActive,DateTime CreatedDate,DateTime? ModifiedDate);
public class ProductRequest{[Required,MaxLength(200)]public string ProductName{get;set;}=string.Empty;public string? Description{get;set;}[Range(0,double.MaxValue)]public decimal Price{get;set;}[Range(0,int.MaxValue)]public int StockQuantity{get;set;}[MaxLength(500),Url]public string? ImageUrl{get;set;}[Range(1,int.MaxValue)]public int CategoryId{get;set;}public bool IsActive{get;set;}=true;}
public class LoginRequest{[Required,EmailAddress]public string Email{get;set;}=string.Empty;[Required]public string Password{get;set;}=string.Empty;}

// Account DTOs
public class RegisterRequest
{
    [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
}
public record AccountDto(int AccountId, string FullName, string Email);

// Cart DTOs
public record CartItemDto(int CartItemId, int ProductId, string ProductName, string? ImageUrl, decimal Price, int Quantity, decimal Subtotal);
public record CartDto(int CartId, List<CartItemDto> Items, decimal Total);
public class AddToCartRequest { [Range(1, int.MaxValue)] public int ProductId { get; set; } [Range(1, int.MaxValue)] public int Quantity { get; set; } = 1; }
public class UpdateCartItemRequest { [Range(1, int.MaxValue)] public int Quantity { get; set; } }

// Order DTOs
public record OrderItemDto(int OrderItemId, int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal Subtotal);
public record OrderDto(int OrderId, DateTime OrderDate, decimal TotalAmount, string Status, List<OrderItemDto> Items);
