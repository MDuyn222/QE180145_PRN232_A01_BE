using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleShop.Service.DTOs;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (!string.Equals(request.Email, adminEmail, StringComparison.OrdinalIgnoreCase)
            || request.Password != adminPassword)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret is missing.");

        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(ClaimTypes.Name, request.Email),
                new Claim(ClaimTypes.Role, "Admin")
            ],
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256));

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            role = "Admin",
            email = request.Email
        });
    }
}
