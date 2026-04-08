using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.PriceList;

public class RepairPriceList : BaseEntity, IAuditableEntity
{
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public int FaultTypeId { get; set; }         // CommonFaultType FK
    public int? ActionTypeId { get; set; }       // CommonActionType FK
    public decimal StandardPrice { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Device.DeviceModel DeviceModel { get; set; } = null!;
    public Device.DeviceModelVariant? DeviceModelVariant { get; set; }
    public CommonFault.CommonFaultType FaultType { get; set; } = null!;
    public CommonFault.CommonActionType? ActionType { get; set; }
}
