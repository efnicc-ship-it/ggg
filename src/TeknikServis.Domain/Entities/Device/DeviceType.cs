using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceType : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty; // Telefon | Tablet | Notebook | Diğer
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<DeviceBrand> Brands { get; set; } = new List<DeviceBrand>();
}
