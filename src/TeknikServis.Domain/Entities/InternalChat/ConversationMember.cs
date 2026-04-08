using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.InternalChat;

public class ConversationMember : BaseEntity
{
    public int ConversationId { get; set; }
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsAdmin { get; set; } = false;   // Grup yöneticisi
    public int UnreadCount { get; set; } = 0;
    public DateTime? LastReadAt { get; set; }

    public Conversation Conversation { get; set; } = null!;
}
