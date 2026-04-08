using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Service;

public class ServicePart : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public int PartId { get; set; }
    public int? PartOrderItemId { get; set; }  // Hangi sipariş kalemindengeldi
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public PartOrderStatus Status { get; set; } = PartOrderStatus.Ordered;
    public bool ReturnedToSupplier { get; set; } = false;
    public bool ReturnedToStock { get; set; } = false;
    public bool IsWrittenOff { get; set; } = false;          // Takma sırasında kırıldı/fireye düştü
    public int? WriteOffId { get; set; }                     // StockWriteOff kaydı FK
    public bool IsReplacementOrder { get; set; } = false;    // Fireye düşen parçanın yerine gelen sipariş
    public int? ReplacesServicePartId { get; set; }          // Hangi servicepart'ın yerine geldi

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
    public Stock.Part Part { get; set; } = null!;
}
