using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TeknikServis.Application.Common.Interfaces;
using TeknikServis.Domain.Entities.Identity;

namespace TeknikServis.Infrastructure.MultiTenancy;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return value != null && int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);

    public Guid TenantId
    {
        get
        {
            var value = User?.FindFirstValue("TenantId");
            return value != null && Guid.TryParse(value, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public int? BranchId
    {
        get
        {
            var value = User?.FindFirstValue("BranchId");
            return value != null && int.TryParse(value, out var id) ? id : null;
        }
    }

    public int? RegionId
    {
        get
        {
            var value = User?.FindFirstValue("RegionId");
            return value != null && int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool CanViewAllBranches
    {
        get
        {
            var role = Role;
            return role == "SuperAdmin" || role == "TenantOwner" || role == "GeneralManager";
        }
    }

    public bool HasPermission(string permission)
    {
        return User?.HasClaim("Permission", permission) ?? false;
    }
}
