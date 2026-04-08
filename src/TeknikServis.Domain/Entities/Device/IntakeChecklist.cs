using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Device;

public class IntakeChecklist : BaseEntity, IAuditableEntity
{
    public string Title { get; set; } = string.Empty;    // "Ekran Durumu", "Güç Açılıyor mu?"
    public string? Description { get; set; }
    public bool IsRequired { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
