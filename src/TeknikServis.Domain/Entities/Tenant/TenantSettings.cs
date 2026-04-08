using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Tenant;

public class TenantSettings : BaseEntity
{
    public new Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;     // "SmsProvider", "WhatsAppEnabled"
    public string Value { get; set; } = string.Empty;   // JSON or plain value
    public string? Description { get; set; }

    public Domain.Entities.Tenant.Tenant Tenant { get; set; } = null!;
}
