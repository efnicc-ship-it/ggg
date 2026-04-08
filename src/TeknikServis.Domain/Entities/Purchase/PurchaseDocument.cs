using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Purchase;

public class PurchaseDocument : BaseEntity, IAuditableEntity
{
    public int PurchaseRecordId { get; set; }
    public string Title { get; set; } = string.Empty;    // "e-devlet IMEI PDF", "Kimlik Fotoğrafı"
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // pdf, jpg, png
    public bool IsEncrypted { get; set; } = false;       // Kimlik fotoğrafı için true

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public PurchaseRecord PurchaseRecord { get; set; } = null!;
}
