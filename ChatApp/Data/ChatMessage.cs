using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Data;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public string? AttachmentUrl { get; set; }

    // Navigation property for recipients (if private/group)
    // If empty, it's a broadcast to all
    public List<ChatRecipient> Recipients { get; set; } = new();
}

public class ChatRecipient
{
    public int Id { get; set; }

    public int ChatMessageId { get; set; }
    [ForeignKey("ChatMessageId")]
    public ChatMessage? ChatMessage { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}
