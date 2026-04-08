using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceBrand : BaseEntity, IAuditableEntity
{
    public int DeviceTypeId { get; set; }
    public string Name { get; set; } = string.Empty; // Apple | Samsung | Xiaomi
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public DeviceType DeviceType { get; set; } = null!;
    public ICollection<DeviceModel> Models { get; set; } = new List<DeviceModel>();
}
