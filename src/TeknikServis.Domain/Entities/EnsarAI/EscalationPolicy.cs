using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.EnsarAI;

public class EscalationPolicy : BaseEntity, IAuditableEntity
{
    public int RuleId { get; set; }
    public int StepOrder { get; set; }           // 1: Teknisyen, 2: BranchManager, 3: GM
    public string TargetRole { get; set; } = string.Empty; // UserRole string
    public int? TargetUserId { get; set; }       // Belirli kullanıcı (opsiyonel)
    public NotificationChannel Channel { get; set; }
    public int WaitMinutes { get; set; } = 60;   // Önceki adımdan sonra bekleme süresi
    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public AiRule Rule { get; set; } = null!;
}
