using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.InternalChat;

public class Message : BaseEntity
{
    public int ConversationId { get; set; }
    public int SenderId { get; set; }  // UserId (0 = Ensar AI)
    public string Content { get; set; } = string.Empty;
    public bool IsSystemMessage { get; set; } = false;   // Ensar AI veya sistem mesajı
    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public int? ReplyToMessageId { get; set; }           // Cevaplanan mesaj

    public Conversation Conversation { get; set; } = null!;
    public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
}
