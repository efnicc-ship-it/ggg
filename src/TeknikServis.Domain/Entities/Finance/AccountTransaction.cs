using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Finance;

public class AccountTransaction : BaseEntity, IAuditableEntity
{
    public int AccountId { get; set; }
    public int? BranchId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // "Debit" | "Credit"
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }  // "ServiceRecord" | "SaleRecord" | "PurchaseRecord" | "Manual"
    public int? ReferenceId { get; set; }
    public string? PaymentMethod { get; set; }  // "Nakit" | "KrediKarti" | "Havale"

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Account Account { get; set; } = null!;
}
