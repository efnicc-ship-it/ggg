using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceModelVariant : BaseEntity, IAuditableEntity
{
    public int ModelId { get; set; }
    public string VariantName { get; set; } = string.Empty; // "128GB", "256GB", "M3 Pro 18GB"
    public string ModelCode { get; set; } = string.Empty;   // A3290, MYD83LL/A — zorunlu
    public string? Storage { get; set; }                    // "128GB", "512GB"
    public string? Ram { get; set; }                        // "8GB", "16GB"
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public DeviceModel Model { get; set; } = null!;
}
