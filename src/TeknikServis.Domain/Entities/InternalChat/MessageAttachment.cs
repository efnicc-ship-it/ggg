using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.InternalChat;

public class MessageAttachment : BaseEntity
{
    public int MessageId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;  // jpg, png, pdf, mp4
    public long FileSize { get; set; }

    public Message Message { get; set; } = null!;
}
