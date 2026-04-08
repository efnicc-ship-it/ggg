using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TeknikServis.Domain.Entities.Identity;

namespace TeknikServis.Api.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [EnableRateLimiting("api")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new { message = "Hesabınız kilitlendi. Lütfen 15 dakika sonra tekrar deneyin." });
            return Unauthorized(new { message = "Geçersiz kullanıcı adı veya şifre." });
        }

        var token = await GenerateJwtToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            token = token.Token,
            expires = token.Expires,
            userId = user.Id,
            userName = user.UserName,
            role = user.Role.ToString()
        });
    }

    private async Task<(string Token, DateTime Expires)> GenerateJwtToken(AppUser user)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var permissions = await GetUserPermissions(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("TenantId", user.TenantId.ToString()),
        };

        if (user.BranchId.HasValue)
            claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));

        if (user.RegionId.HasValue)
            claims.Add(new Claim("RegionId", user.RegionId.Value.ToString()));

        foreach (var permission in permissions)
            claims.Add(new Claim("Permission", permission));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private async Task<IEnumerable<string>> GetUserPermissions(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Where(c => c.Type == "Permission").Select(c => c.Value);
    }

    public record LoginRequest(string UserName, string Password);
}
