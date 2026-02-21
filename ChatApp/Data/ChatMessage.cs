using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Data;

/// <summary>
/// Entity representing a chat message stored in the database.
/// </summary>
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

    // Navigation property for message reactions
    public List<MessageReaction> Reactions { get; set; } = new();
}

/// <summary>
/// Entity representing a specific recipient of a private or group chat message.
/// </summary>
public class ChatRecipient
{
    public int Id { get; set; }

    public int ChatMessageId { get; set; }
    [ForeignKey("ChatMessageId")]
    public ChatMessage? ChatMessage { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Entity representing a user's emoji reaction to a specific chat message.
/// </summary>
public class MessageReaction
{
    public int Id { get; set; }

    public int ChatMessageId { get; set; }
    [ForeignKey("ChatMessageId")]
    public ChatMessage? ChatMessage { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Emoji { get; set; } = string.Empty;
}
