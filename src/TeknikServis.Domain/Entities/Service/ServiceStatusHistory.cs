using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Service;

public class ServiceStatusHistory : BaseEntity
{
    public int ServiceRecordId { get; set; }
    public ServiceStatus OldStatus { get; set; }
    public ServiceStatus NewStatus { get; set; }
    public string? Note { get; set; }
    public int ChangedBy { get; set; } // UserId

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
