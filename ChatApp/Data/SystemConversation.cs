using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp.Data;

/// <summary>
/// Entity representing an explicit conversation thread.
/// </summary>
public class SystemConversation
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsGroupChat { get; set; }

    public string? Title { get; set; }

    public List<SystemConversationParticipant> Participants { get; set; } = new();
    public List<ChatMessage> Messages { get; set; } = new();
}

/// <summary>
/// Entity representing a participant's state within a conversation thread.
/// </summary>
public class SystemConversationParticipant
{
    public int Id { get; set; }

    public int SystemConversationId { get; set; }
    [ForeignKey("SystemConversationId")]
    public SystemConversation? Conversation { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public bool HasLeft { get; set; }

    public DateTime? LeftAt { get; set; }
}
