using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Customer;

public class CustomerSurveyResponse : BaseEntity
{
    public int CustomerId { get; set; }
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
    public string? Context { get; set; } // "Service" | "Sale"
    public int? RelatedRecordId { get; set; }
    public bool GoogleReviewPromptSent { get; set; }

    public Customer Customer { get; set; } = null!;
}
