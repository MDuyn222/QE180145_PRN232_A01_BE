using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "User")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private int AccountId => int.Parse(User.FindFirstValue("accountId")!);

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout()
    {
        var (order, error) = await orderService.CheckoutAsync(AccountId);
        return order is not null
            ? Ok(new { message = "Order placed successfully.", order })
            : BadRequest(new { message = error });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders() =>
        Ok(await orderService.GetOrdersAsync(AccountId));

    [HttpGet("{orderId:int}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var order = await orderService.GetOrderAsync(orderId, AccountId);
        return order is null ? NotFound(new { message = "Order not found." }) : Ok(order);
    }
}
