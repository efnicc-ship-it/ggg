using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Identity;

namespace TeknikServis.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IApplicationDbContext _db;

    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IApplicationDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet("/giris")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("/giris")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByNameAsync(model.UserName)
                ?? await _userManager.FindByEmailAsync(model.UserName);

        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Tüm rolleri claim'e ekle
            var roles = await _userManager.GetRolesAsync(user);
            var additionalClaims = new List<Claim>
            {
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("Role", user.Role.ToString()),
            };
            if (user.BranchId.HasValue)
                additionalClaims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));

            foreach (var role in roles)
                additionalClaims.Add(new Claim(ClaimTypes.Role, role));

            await _userManager.AddClaimsAsync(user, additionalClaims.Where(c =>
                !_userManager.GetClaimsAsync(user).Result.Any(existing => existing.Type == c.Type && existing.Value == c.Value)));

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Hesabınız kilitlendi. 15 dakika sonra tekrar deneyin.");
            return View(model);
        }

        ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
        return View(model);
    }

    [HttpPost("/cikis")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet("/erisim-engellendi")]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}

public class LoginViewModel
{
    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string UserName { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Şifre zorunludur.")]
    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
