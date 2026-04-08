namespace TeknikServis.Domain.Entities.Tenant;

public class Tenant
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string SubscriptionPlan { get; set; } = "Starter"; // Starter | Professional | Enterprise
    public bool IsActive { get; set; } = true;
    public DateTime? TrialEndsAt { get; set; }
    public string? SettingsJson { get; set; } // JSON: tenant-specific configuration
    public DateTime CreatedAt { get; set; }

    public ICollection<TenantModule> Modules { get; set; } = new List<TenantModule>();
}
