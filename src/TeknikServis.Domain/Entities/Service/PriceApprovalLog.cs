using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class PriceApprovalLog : BaseEntity
{
    public int ServiceRecordId { get; set; }
    public decimal QuotedPrice { get; set; }
    public string Channel { get; set; } = string.Empty;  // "SMS" | "WhatsApp" | "Email"
    public DateTime SentAt { get; set; }
    public bool? Approved { get; set; }                   // null: cevap yok, true/false
    public DateTime? RespondedAt { get; set; }
    public string? CustomerNote { get; set; }
    public int SentBy { get; set; }  // UserId

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
