using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.CommonFault;

public class CommonActionType : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;  // "Ekran Değişimi", "Pil Değişimi"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
