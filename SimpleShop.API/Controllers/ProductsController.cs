using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleShop.Service.DTOs;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActive() =>
        Ok(await service.GetAllAsync(false));

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync(true));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await service.GetAsync(id, false);
        return product is null ? NotFound(new { message = "Product not found." }) : Ok(product);
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ProductRequest request)
    {
        try
        {
            var product = await service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SoftDelete(int id) =>
        await service.SoftDeleteAsync(id)
            ? NoContent()
            : NotFound(new { message = "Product not found." });
}
