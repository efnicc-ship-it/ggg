using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.InternalChat;

public class Conversation : BaseEntity, IAuditableEntity
{
    public string? Title { get; set; }          // Grup adı (birebir için null)
    public bool IsGroup { get; set; } = false;
    public int? BranchId { get; set; }
    public DateTime LastMessageAt { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ICollection<ConversationMember> Members { get; set; } = new List<ConversationMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
