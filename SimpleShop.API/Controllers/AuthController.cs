using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SimpleShop.Service.DTOs;
using SimpleShop.Service.Services;

namespace SimpleShop.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration, IAccountService accountService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var (account, error) = await accountService.RegisterAsync(request);
        if (account is null) return BadRequest(new { message = error });
        return Ok(new { message = "Registration successful.", account });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Check admin first
        var adminEmail = configuration["Admin:Email"];
        var adminPassword = configuration["Admin:Password"];

        if (string.Equals(request.Email, adminEmail, StringComparison.OrdinalIgnoreCase)
            && request.Password == adminPassword)
        {
            return Ok(new
            {
                token = GenerateToken(request.Email, "Admin", 0),
                role = "Admin",
                email = request.Email,
                fullName = "Admin"
            });
        }

        // User login
        var account = await accountService.ValidateAsync(request.Email, request.Password);
        if (account is null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new
        {
            token = GenerateToken(account.Email, "User", account.AccountId),
            role = "User",
            email = account.Email,
            fullName = account.FullName,
            accountId = account.AccountId
        });
    }

    private string GenerateToken(string email, string role, int accountId)
    {
        var secret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT secret is missing.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Role, role)
        };
        if (accountId > 0)
            claims.Add(new Claim("accountId", accountId.ToString()));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var email = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var accountIdStr = User.FindFirstValue("accountId");
        return Ok(new { email, role, accountId = accountIdStr });
    }
}
