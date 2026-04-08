using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Dealer;

public class DealerDevice : BaseEntity, IAuditableEntity
{
    public int DealerId { get; set; }
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public string? Imei1 { get; set; }
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? Color { get; set; }
    public string? FaultDescription { get; set; }
    public string Status { get; set; } = "Received";  // "Received" | "InService" | "Ready" | "Returned"
    public int? LinkedServiceRecordId { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Dealer Dealer { get; set; } = null!;
    public Device.DeviceModel DeviceModel { get; set; } = null!;
    public Device.DeviceModelVariant? DeviceModelVariant { get; set; }
}
