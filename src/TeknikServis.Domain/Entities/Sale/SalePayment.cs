using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Sale;

public class SalePayment : BaseEntity, IAuditableEntity
{
    public int SaleRecordId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // "Nakit" | "KrediKarti" | "Havale" | "Online"
    public string? Reference { get; set; }
    public string? Note { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
}
