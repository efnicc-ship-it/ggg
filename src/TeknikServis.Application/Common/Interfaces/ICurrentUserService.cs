namespace TeknikServis.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    Guid TenantId { get; }
    int? BranchId { get; }
    int? RegionId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool CanViewAllBranches { get; }
    bool HasPermission(string permission);
}
