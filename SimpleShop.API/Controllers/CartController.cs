using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleShop.Service.DTOs;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "User")]
public class CartController(ICartService cartService) : ControllerBase
{
    private int AccountId => int.Parse(User.FindFirstValue("accountId")!);

    [HttpGet]
    public async Task<IActionResult> GetCart() =>
        Ok(await cartService.GetCartAsync(AccountId));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddToCartRequest request)
    {
        var (success, error) = await cartService.AddToCartAsync(AccountId, request);
        return success ? Ok(new { message = "Item added to cart." }) : BadRequest(new { message = error });
    }

    [HttpPut("items/{cartItemId:int}")]
    public async Task<IActionResult> UpdateItem(int cartItemId, UpdateCartItemRequest request)
    {
        var (success, error) = await cartService.UpdateCartItemAsync(AccountId, cartItemId, request);
        if (!success) return error == "Item not found." || error == "Cart not found."
            ? NotFound(new { message = error })
            : BadRequest(new { message = error });
        return NoContent();
    }

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var (success, error) = await cartService.RemoveCartItemAsync(AccountId, cartItemId);
        return success ? NoContent() : NotFound(new { message = error });
    }
}
