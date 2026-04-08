using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

public class PartCompatibility : BaseEntity, IAuditableEntity
{
    public int PartId { get; set; }
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Part Part { get; set; } = null!;
    public Device.DeviceModel DeviceModel { get; set; } = null!;
    public Device.DeviceModelVariant? DeviceModelVariant { get; set; }
}
