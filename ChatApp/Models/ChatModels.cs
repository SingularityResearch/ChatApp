using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatApp.Models;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class ConversationViewModel
{
    public string ConversationId { get; set; } = string.Empty; // Unique Key based on sorted participants
    public List<string> ParticipantIds { get; set; } = new();
    public string DisplayName { get; set; } = string.Empty;
    public List<ChatMessage> Messages { get; set; } = new();
    public DateTime LastActivity => Messages.LastOrDefault()?.Timestamp ?? DateTime.MinValue;
    public bool IsGroup => ParticipantIds.Count > 2; // Me + 2 others = 3 total
}

public class ChatMessage
{
    public int Id { get; set; }
    public string User { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsMine { get; set; }
    public string? SenderId { get; set; }

    // UI State
    public bool IsEditing { get; set; }
    public string EditBuffer { get; set; } = string.Empty;
}

public class UserActivityDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public DateTime? LastActive { get; set; }
}

public class UserMessageDetailDto
{
    public int MessageId { get; set; }
    public string Direction { get; set; } = string.Empty; // "Sent" or "Received"
    public string OtherParties { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
