using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleShop.Service.DTOs;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService service) : ControllerBase
{
    // =========================
    // PUBLIC
    // =========================

    [HttpGet]
    public async Task<IActionResult> GetActive() =>
        Ok(await service.GetAllAsync(false));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await service.GetAsync(id, false);
        return category is null
            ? NotFound(new { message = "Category not found." })
            : Ok(category);
    }

    // =========================
    // AUTHENTICATED USER MANAGEMENT
    // =========================

    [HttpGet("all")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetAll() =>
    Ok(await service.GetAllAsync(true));

    [HttpGet("search")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Search([FromQuery] string name = "") =>
    Ok(await service.SearchAsync(name));

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        var category = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = category.CategoryId }, category);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Update(int id, CategoryRequest request) =>
        await service.UpdateAsync(id, request)
            ? NoContent()
            : NotFound(new { message = "Category not found." });

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);

        if (result.Success)
            return NoContent();

        return result.Error == "Category not found."
            ? NotFound(new { message = result.Error })
            : BadRequest(new { message = result.Error });
    }
}