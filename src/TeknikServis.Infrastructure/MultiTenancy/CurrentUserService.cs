using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TeknikServis.Application.Common.Interfaces;

namespace TeknikServis.Infrastructure.MultiTenancy;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? UserName => User?.FindFirstValue(ClaimTypes.Name);
    public string? Role => User?.FindFirstValue(ClaimTypes.Role);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid TenantId
    {
        get
        {
            var claim = User?.FindFirstValue("TenantId");
            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }

    public int? BranchId
    {
        get
        {
            var claim = User?.FindFirstValue("BranchId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public int? RegionId
    {
        get
        {
            var claim = User?.FindFirstValue("RegionId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }

    public bool CanViewAllBranches => Role is "SuperAdmin" or "TenantOwner" or "GeneralManager" or "RegionalManager";

    public bool HasPermission(string permission)
    {
        return User?.HasClaim("permission", permission) ?? false;
    }
}
