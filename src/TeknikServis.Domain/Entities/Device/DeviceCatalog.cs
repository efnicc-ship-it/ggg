using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceCatalog : BaseEntity, IAuditableEntity
{
    public int ModelId { get; set; }
    public int? VariantId { get; set; }               // Null ise modelin kendisi
    public string? ModelCode { get; set; }            // Katalog kodu
    public string? Description { get; set; }
    public string? Processor { get; set; }
    public string? ScreenSize { get; set; }
    public string? BatteryCapacity { get; set; }
    public string? CameraSpec { get; set; }
    public string? Os { get; set; }
    public string? ConnectivitySpec { get; set; }
    public string? AdditionalSpecs { get; set; }      // JSON — ek teknik özellikler
    public string? ImagePath { get; set; }
    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public DeviceModel Model { get; set; } = null!;
    public DeviceModelVariant? Variant { get; set; }
    public ICollection<DeviceInventory> Inventories { get; set; } = new List<DeviceInventory>();
}
