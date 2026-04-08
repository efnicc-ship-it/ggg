using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Sale;

public class OnlinePaymentRecord : BaseEntity, IAuditableEntity
{
    public int SaleRecordId { get; set; }
    public string Provider { get; set; } = string.Empty;  // "iyzico" | "PayTR"
    public string PaymentToken { get; set; } = string.Empty;
    public string? PaymentUrl { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;    // "Pending" | "Success" | "Failed" | "Refunded"
    public string? ProviderReference { get; set; }        // Sağlayıcı ref no
    public DateTime? PaidAt { get; set; }
    public string? FailReason { get; set; }
    public string Channel { get; set; } = string.Empty;   // "SMS" | "WhatsApp" — link gönderim kanalı

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
}
