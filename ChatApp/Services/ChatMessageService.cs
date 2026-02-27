using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;


namespace ChatApp.Services;

/// <summary>
/// Service interface for managing chat messages and user activity.
/// </summary>
public interface IChatMessageService
{
    /// <summary>
    /// Saves a new chat message to the database.
    /// </summary>
    /// <param name="conversationId">The ID of the SystemConversation this message belongs to.</param>
    /// <param name="senderId">The ID of the user sending the message.</param>
    /// <param name="senderName">The name of the user sending the message.</param>
    /// <param name="message">The text content of the message.</param>
    /// <param name="attachmentUrl">An optional URL for a file attachment.</param>
    /// <returns>The ID of the newly saved message.</returns>
    Task<int> SaveMessageAsync(int conversationId, string senderId, string senderName, string message, string? attachmentUrl);

    /// <summary>
    /// Gets or creates a SystemConversation involving the exact set of participants.
    /// </summary>
    Task<SystemConversation> GetOrCreateConversationAsync(List<string> participantIds);

    /// <summary>
    /// Gets the IDs of all active participants in a conversation.
    /// </summary>
    Task<List<string>> GetActiveParticipantIdsAsync(int conversationId);

    /// <summary>
    /// Marks a user as having left a conversation.
    /// </summary>
    Task<bool> LeaveConversationAsync(int conversationId, string userId);

    /// <summary>
    /// Updates the title of a specific conversation.
    /// </summary>
    Task<bool> UpdateConversationTitleAsync(int conversationId, string title);

    /// <summary>
    /// Edits an existing message's text content.
    /// </summary>
    /// <param name="messageId">The ID of the message to edit.</param>
    /// <param name="currentUserId">The ID of the user attempting the edit.</param>
    /// <param name="newContent">The new text content for the message.</param>
    /// <returns>True if the edit was successful, false otherwise.</returns>
    Task<bool> EditMessageAsync(int messageId, string currentUserId, string newContent);

    /// <summary>
    /// Deletes a message from the database.
    /// </summary>
    /// <param name="messageId">The ID of the message to delete.</param>
    /// <param name="currentUserId">The ID of the user attempting the deletion.</param>
    /// <returns>True if the deletion was successful, false otherwise.</returns>
    Task<bool> DeleteMessageAsync(int messageId, string currentUserId);

    /// <summary>
    /// Retrieves the chat conversations visible to a specific user, along with their messages.
    /// </summary>
    /// <param name="currentUserId">The ID of the user requesting history.</param>
    /// <returns>A list of conversations the user is a participant of.</returns>
    Task<List<SystemConversation>> GetUserConversationsAsync(string currentUserId);

    /// <summary>
    /// Generates a report of user activity including message counts and last active times.
    /// </summary>
    /// <returns>A list of user activity statistics.</returns>
    Task<List<UserActivityDto>> GetUserActivityReportAsync();

    /// <summary>
    /// Gets detailed message statistics for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to get details for.</param>
    /// <returns>A list of message details for the user.</returns>
    Task<List<UserMessageDetailDto>> GetUserMessageDetailsAsync(string userId);

    /// <summary>
    /// Retrieves a specific conversation by its ID.
    /// </summary>
    /// <param name="conversationId">The ID of the conversation to retrieve.</param>
    /// <returns>The SystemConversation with its history, or null if not found.</returns>
    Task<SystemConversation?> GetConversationByIdAsync(int conversationId);

    /// <summary>
    /// Adds a reaction to a specified message.
    /// </summary>
    /// <param name="messageId">The ID of the message to react to.</param>
    /// <param name="userId">The ID of the user adding the reaction.</param>
    /// <param name="emoji">The emoji shortcode representing the reaction.</param>
    /// <returns>True if the reaction was successfully added, false if it already exists or the message is not found.</returns>
    Task<bool> AddReactionAsync(int messageId, string userId, string emoji);

    /// <summary>
    /// Removes a user's reaction from a specified message.
    /// </summary>
    /// <param name="messageId">The ID of the message to remove the reaction from.</param>
    /// <param name="userId">The ID of the user removing the reaction.</param>
    /// <param name="emoji">The emoji shortcode representing the reaction to remove.</param>
    /// <returns>True if the reaction was successfully removed, false otherwise.</returns>
    Task<bool> RemoveReactionAsync(int messageId, string userId, string emoji);
}

/// <summary>
/// Implementation of the <see cref="IChatMessageService"/> for database operations.
/// </summary>
/// <param name="scopeFactory">A factory for creating dependency injection scopes.</param>
public class ChatMessageService(IServiceScopeFactory scopeFactory) : IChatMessageService
{

    /// <inheritdoc/>
    public async Task<int> SaveMessageAsync(int conversationId, string senderId, string senderName, string message, string? attachmentUrl)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var sanitizer = scope.ServiceProvider.GetRequiredService<HtmlSanitizerService>();
        string sanitizedMessage = sanitizer.Sanitize(message);

        var msgEntity = new Data.ChatMessage
        {
            SystemConversationId = conversationId,
            SenderId = senderId,
            SenderName = senderName,
            Message = sanitizedMessage,
            Timestamp = DateTime.Now,
            AttachmentUrl = attachmentUrl
        };

        db.ChatMessages.Add(msgEntity);
        await db.SaveChangesAsync();
        return msgEntity.Id;
    }

    /// <inheritdoc/>
    public async Task<SystemConversation> GetOrCreateConversationAsync(List<string> participantIds)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Load all conversations that contain the exact set of participants
        var conversations = await db.SystemConversations
            .Include(c => c.Participants)
            .Where(c => c.Participants.Count == participantIds.Count)
            .ToListAsync();

        var existing = conversations.FirstOrDefault(c =>
            c.Participants.All(p => participantIds.Contains(p.UserId)));

        if (existing != null)
        {
            // Un-leave any participants if they are coming back
            var leftParticipants = existing.Participants.Where(p => p.HasLeft);
            foreach (var p in leftParticipants)
            {
                p.HasLeft = false;
                p.LeftAt = null;
            }
            if (leftParticipants.Any())
            {
                await db.SaveChangesAsync();
            }
            return existing;
        }

        var newConv = new SystemConversation
        {
            IsGroupChat = participantIds.Count > 2,
            Participants = participantIds.Select(uid => new SystemConversationParticipant { UserId = uid }).ToList()
        };

        db.SystemConversations.Add(newConv);
        await db.SaveChangesAsync();
        return newConv;
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetActiveParticipantIdsAsync(int conversationId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.SystemConversationParticipants
            .Where(p => p.SystemConversationId == conversationId && !p.HasLeft)
            .Select(p => p.UserId)
            .ToListAsync();
    }

    public async Task<bool> LeaveConversationAsync(int conversationId, string userId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var participant = await db.SystemConversationParticipants
            .FirstOrDefaultAsync(p => p.SystemConversationId == conversationId && p.UserId == userId);

        if (participant != null)
        {
            if (!participant.HasLeft)
            {
                participant.HasLeft = true;
                participant.LeftAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return true;
            }
        }
        
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateConversationTitleAsync(int conversationId, string title)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var conversation = await db.SystemConversations.FindAsync(conversationId);
        if (conversation != null)
        {
            conversation.Title = title;
            await db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> EditMessageAsync(int messageId, string currentUserId, string newContent)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var msg = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId && m.SenderId == currentUserId);

        if (msg != null)
        {
            var sanitizer = scope.ServiceProvider.GetRequiredService<HtmlSanitizerService>();
            msg.Message = sanitizer.Sanitize(newContent);
            await db.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteMessageAsync(int messageId, string currentUserId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var msg = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId && m.SenderId == currentUserId);

        if (msg != null)
        {
            db.ChatMessages.Remove(msg);
            await db.SaveChangesAsync();
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> AddReactionAsync(int messageId, string userId, string emoji)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if message exists
        var msgExists = await db.ChatMessages.AnyAsync(m => m.Id == messageId);
        if (!msgExists) return false;

        // Check if reaction already exists
        var existing = await db.MessageReactions
            .SingleOrDefaultAsync(r => r.ChatMessageId == messageId && r.UserId == userId && r.Emoji == emoji);

        if (existing == null)
        {
            db.MessageReactions.Add(new MessageReaction
            {
                ChatMessageId = messageId,
                UserId = userId,
                Emoji = emoji
            });
            await db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveReactionAsync(int messageId, string userId, string emoji)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var existing = await db.MessageReactions
            .SingleOrDefaultAsync(r => r.ChatMessageId == messageId && r.UserId == userId && r.Emoji == emoji);

        if (existing != null)
        {
            db.MessageReactions.Remove(existing);
            await db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<List<SystemConversation>> GetUserConversationsAsync(string currentUserId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var conversations = await db.SystemConversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Reactions)
            .Where(c => c.Participants.Any(p => p.UserId == currentUserId && !p.HasLeft))
            .ToListAsync();

        return conversations;
    }

    /// <inheritdoc/>
    public async Task<List<UserActivityDto>> GetUserActivityReportAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Query Users and Left Join with ChatMessages to aggregate data
        var userActivity = await db.Users
            .Select(u => new UserActivityDto
            {
                UserId = u.Id,
                UserName = u.UserName ?? "Unknown",
                MessageCount = db.ChatMessages.Count(m => m.SenderId == u.Id),
                LastActive = db.ChatMessages
                            .Where(m => m.SenderId == u.Id)
                            .OrderByDescending(m => m.Timestamp)
                            .Select(m => (DateTime?)m.Timestamp)
                            .FirstOrDefault()
            })
            .ToListAsync();

        return userActivity;
    }

    /// <inheritdoc/>
    public async Task<List<UserMessageDetailDto>> GetUserMessageDetailsAsync(string userId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var messages = await db.ChatMessages
            .Include(m => m.Conversation)
                .ThenInclude(sc => sc!.Participants)
            .Where(m => m.SenderId == userId || (m.Conversation != null && m.Conversation.Participants.Any(p => p.UserId == userId)))
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        var details = new List<UserMessageDetailDto>();

        foreach (var m in messages)
        {
            var otherParticipantIds = m.Conversation?.Participants
                .Where(p => p.UserId != m.SenderId)
                .Select(p => p.UserId)
                .ToList() ?? new List<string>();

            var otherPartiesString = otherParticipantIds.Count > 0 
                ? string.Join(", ", otherParticipantIds) 
                : m.SenderId ?? "Unknown"; // For "Notes to Self"

            details.Add(new UserMessageDetailDto
            {
                MessageId = m.Id,
                SystemConversationId = m.Conversation?.Id ?? 0,
                Direction = m.SenderId == userId ? "Sent" : "Received",
                OtherParties = otherPartiesString,
                Timestamp = m.Timestamp
            });
        }

        // We should map OtherParties to actual UserNames if possible
        var userIds = details.SelectMany(d => d.OtherParties.Split(", ", StringSplitOptions.RemoveEmptyEntries)).Distinct().ToList();
        var userNames = await db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName);

        foreach (var d in details)
        {
            var ids = d.OtherParties.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            var names = ids.Select(id => userNames.TryGetValue(id, out var name) ? name : "Unknown").ToList();
            d.OtherParties = string.Join(", ", names);
        }

        return details;
    }

    /// <inheritdoc/>
    public async Task<SystemConversation?> GetConversationByIdAsync(int conversationId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.SystemConversations
            .Include(c => c.Participants)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Reactions)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }
}
