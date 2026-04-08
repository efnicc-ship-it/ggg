using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class ServicePayment : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // "Nakit" | "KrediKarti" | "Havale" | "Online"
    public string? Reference { get; set; }                    // Kart/havale ref no
    public string? Note { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
