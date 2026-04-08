using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Sale;

public class SaleRecord : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string RecordNumber { get; set; } = string.Empty; // SAT-2024-001234
    public int? CustomerId { get; set; }       // Aksesuar satışında opsiyonel
    public int? BranchId { get; set; }
    public int? PurchaseRecordId { get; set; } // Cihaz alışıyla bağlantı

    public bool IsAccessorySaleOnly { get; set; } = false;

    // Toplam
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalAmount { get; set; }

    // Ödeme
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal TotalPaid { get; set; } = 0;
    public bool DeferredPayment { get; set; } = false;
    public DateTime? DeferredDueDate { get; set; }

    // Garanti
    public int? WarrantyDays { get; set; }
    public DateTime? WarrantyExpiresAt { get; set; }

    // Belgeler
    public string? ContractPdfPath { get; set; }

    // Online ödeme
    public string? OnlinePaymentLinkUrl { get; set; }
    public string? OnlinePaymentReference { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public Customer.Customer? Customer { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<SalePayment> Payments { get; set; } = new List<SalePayment>();
    public WarrantyRecord? WarrantyRecord { get; set; }
    public ICollection<DiscountRecord> Discounts { get; set; } = new List<DiscountRecord>();
    public ICollection<OnlinePaymentRecord> OnlinePayments { get; set; } = new List<OnlinePaymentRecord>();
}
