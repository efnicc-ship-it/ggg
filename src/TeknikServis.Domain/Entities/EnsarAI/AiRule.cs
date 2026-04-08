using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.EnsarAI;

public class AiRule : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TriggerType { get; set; } = string.Empty;   // "Scheduled" | "Event"
    public string? CronExpression { get; set; }               // Scheduled için
    public string? EventType { get; set; }                    // Event türü
    public string ConditionJson { get; set; } = string.Empty; // Kural koşulu (JSON)
    public string ActionsJson { get; set; } = string.Empty;   // Aksiyon listesi (JSON)
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;
    public int CooldownMinutes { get; set; } = 60;            // Aynı kayıt için tekrar tetikleme süresi
    public bool IsActive { get; set; } = true;
    public int TriggerCount { get; set; } = 0;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<AiAlert> Alerts { get; set; } = new List<AiAlert>();
}
