using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Supplier;

public class Supplier : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? WhatsAppPhone { get; set; }           // Sipariş için WA numarası
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AutoSendWhatsApp { get; set; } = false;  // Sipariş WA toggle
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public ICollection<PartOrder> Orders { get; set; } = new List<PartOrder>();
    public ICollection<SupplierPartPrice> PartPrices { get; set; } = new List<SupplierPartPrice>();
}
