using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class ServiceNote : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = true;  // true: sadece çalışanlar görür

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
