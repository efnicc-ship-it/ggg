using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Customer;

public class CustomerRating : BaseEntity
{
    public int CustomerId { get; set; }
    public int RatedBy { get; set; } // UserId
    public int Score { get; set; }   // 1-5
    public string? Comment { get; set; }
    public string? Context { get; set; } // "Service" | "Purchase" | "Sale"
    public int? RelatedRecordId { get; set; }

    public Customer Customer { get; set; } = null!;
}
