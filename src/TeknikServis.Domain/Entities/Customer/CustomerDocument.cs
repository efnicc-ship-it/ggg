using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Customer;

public class CustomerDocument : BaseEntity, IAuditableEntity
{
    public int CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // pdf, jpg, png
    public bool IsEncrypted { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Customer Customer { get; set; } = null!;
}
