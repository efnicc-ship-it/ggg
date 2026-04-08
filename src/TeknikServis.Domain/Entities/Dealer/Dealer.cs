using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Dealer;

public class Dealer : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string CompanyName { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public decimal CreditLimit { get; set; } = 0;
    public decimal CurrentBalance { get; set; } = 0;  // (+) alacak, (-) borç
    public bool IsActive { get; set; } = true;
    public int? UserId { get; set; }  // Bayi portal login kullanıcısı

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public ICollection<DealerDevice> Devices { get; set; } = new List<DealerDevice>();
}
