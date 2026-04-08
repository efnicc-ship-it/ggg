using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.EnsarAI;

public class AiAlert : BaseEntity
{
    public int RuleId { get; set; }
    public AlertSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }     // "ServiceRecord" | "StockItem" | "User"
    public int? ReferenceId { get; set; }
    public int? AssignedToUserId { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public int? ResolvedBy { get; set; }
    public string? ResolutionNote { get; set; }
    public bool NotificationSent { get; set; } = false;

    public AiRule Rule { get; set; } = null!;
}
