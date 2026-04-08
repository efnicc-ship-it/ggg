using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Sale;

/// <summary>
/// Hurda satışı — hasar gören veya servis sonrası işe yaramaz hale gelen cihaz/parça satışı.
/// Ayrı tutulur çünkü hurda fiyatı çok düşüktür ve muhasebe kaydı farklıdır.
/// </summary>
public class ScrapSaleRecord : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string RecordNumber { get; set; } = string.Empty;  // HRD-2024-001234
    public int? BranchId { get; set; }
    public int? BuyerId { get; set; }            // Dışarıya satan müşteri/firma adı
    public string? BuyerName { get; set; }       // Kayıtsız alıcı
    public string? BuyerPhone { get; set; }

    // Ne satıldı (biri dolu olur)
    public int? DeviceInventoryId { get; set; }  // Hurda cihaz
    public int? PartId { get; set; }             // Hurda parça

    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal SalePrice { get; set; }
    public decimal OriginalCost { get; set; }    // Alış maliyeti — kayıp hesabı için
    public decimal LossAmount { get; set; }      // OriginalCost - SalePrice

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public TeknikServis.Domain.Entities.Device.DeviceInventory? DeviceInventory { get; set; }
    public TeknikServis.Domain.Entities.Stock.Part? Part { get; set; }
}
