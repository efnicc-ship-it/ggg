using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Finance;

public class Account : BaseEntity, IAuditableEntity
{
    public string Code { get; set; } = string.Empty;   // Hesap kodu (100, 101, 120...)
    public string Name { get; set; } = string.Empty;   // "Kasa", "Banka", "Müşteri Alacakları"
    public string AccountType { get; set; } = string.Empty; // "Cash" | "Bank" | "Receivable" | "Payable" | "Revenue" | "Expense"
    public int? BranchId { get; set; }
    public decimal Balance { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<AccountTransaction> Transactions { get; set; } = new List<AccountTransaction>();
}
