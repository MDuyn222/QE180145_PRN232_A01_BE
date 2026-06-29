using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SimpleShop.Service.DTOs;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    private int? AccountId =>
        int.TryParse(User.FindFirstValue("accountId"), out var accountId) ? accountId : null;

    [HttpGet]
public async Task<IActionResult> GetActive() =>
    Ok(await service.GetAllAsync(false));

[HttpGet("{id:int}")]
public async Task<IActionResult> GetById(int id)
{
    var product = await service.GetAsync(id, false);
    return product is null
        ? NotFound(new { message = "Product not found." })
        : Ok(product);
}

[HttpGet("category/{categoryId:int}")]
public async Task<IActionResult> GetByCategory(int categoryId) =>
    Ok(await service.GetByCategoryAsync(categoryId));

[HttpGet("search")]
public async Task<IActionResult> Search(
    [FromQuery] string? name,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice,
    [FromQuery] int? categoryId)
{
    try
    {
        return Ok(await service.SearchAsync(name, minPrice, maxPrice, categoryId));
    }
    catch (ArgumentException exception)
    {
        return BadRequest(new { message = exception.Message });
    }
}



    [HttpGet("all")]
[Authorize(Roles = "User,Admin")]
public async Task<IActionResult> GetAll() =>
    Ok(await service.GetAllAsync(true));

[HttpPost]
[Authorize(Roles = "User,Admin")]
public async Task<IActionResult> Create(ProductRequest request)
{
    try
    {
        var product = await service.CreateAsync(request, AccountId);
        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }
    catch (ArgumentException exception)
    {
        return BadRequest(new { message = exception.Message });
    }
}

[HttpPut("{id:int}")]
[Authorize(Roles = "User,Admin")]
public async Task<IActionResult> Update(int id, ProductRequest request)
{
    try
    {
        return await service.UpdateAsync(id, request)
            ? NoContent()
            : NotFound(new { message = "Product not found." });
    }
    catch (ArgumentException exception)
    {
        return BadRequest(new { message = exception.Message });
    }
}

[HttpDelete("{id:int}")]
[Authorize(Roles = "User,Admin")]
public async Task<IActionResult> SoftDelete(int id) =>
    await service.SoftDeleteAsync(id)
        ? NoContent()
        : NotFound(new { message = "Product not found." });
}