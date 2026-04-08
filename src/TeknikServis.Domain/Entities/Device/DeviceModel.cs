using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceModel : BaseEntity, IAuditableEntity
{
    public int BrandId { get; set; }
    public string Name { get; set; } = string.Empty; // iPhone 15 Pro
    public string? ModelCode { get; set; }            // Varyantsız modeller için tek kod (opsiyonel)
    public bool HasVariants { get; set; } = false;    // true ise DeviceModelVariant tablosuna bak
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public DeviceBrand Brand { get; set; } = null!;
    public ICollection<DeviceModelVariant> Variants { get; set; } = new List<DeviceModelVariant>();
    public ICollection<DeviceCatalog> Catalogs { get; set; } = new List<DeviceCatalog>();
}
