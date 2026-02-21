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
    /// <param name="senderId">The ID of the user sending the message.</param>
    /// <param name="senderName">The name of the user sending the message.</param>
    /// <param name="message">The text content of the message.</param>
    /// <param name="recipientIds">An optional list of recipient user IDs.</param>
    /// <param name="attachmentUrl">An optional URL for a file attachment.</param>
    /// <returns>The ID of the newly saved message.</returns>
    Task<int> SaveMessageAsync(string senderId, string senderName, string message, List<string>? recipientIds, string? attachmentUrl);

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
    /// Retrieves the chat history visible to a specific user.
    /// </summary>
    /// <param name="currentUserId">The ID of the user requesting history.</param>
    /// <returns>A list of messages the user is allowed to see.</returns>
    Task<List<Data.ChatMessage>> GetMessageHistoryAsync(string currentUserId);

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
    public async Task<int> SaveMessageAsync(string senderId, string senderName, string message, List<string>? recipientIds, string? attachmentUrl)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var msgEntity = new Data.ChatMessage
        {
            SenderId = senderId,
            SenderName = senderName,
            Message = message,
            Timestamp = DateTime.Now,
            AttachmentUrl = attachmentUrl
        };

        if (recipientIds != null && recipientIds.Count > 0)
        {
            msgEntity.Recipients = [.. recipientIds.Select(uid => new ChatRecipient { UserId = uid })];
        }

        db.ChatMessages.Add(msgEntity);
        await db.SaveChangesAsync();
        return msgEntity.Id;
    }

    /// <inheritdoc/>
    public async Task<bool> EditMessageAsync(int messageId, string currentUserId, string newContent)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var msg = await db.ChatMessages.SingleOrDefaultAsync(m => m.Id == messageId && m.SenderId == currentUserId);

        if (msg != null)
        {
            msg.Message = newContent;
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
    public async Task<List<Data.ChatMessage>> GetMessageHistoryAsync(string currentUserId)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var history = await db.ChatMessages
            .Include(m => m.Recipients)
            .Include(m => m.Reactions)
            .Where(m => !m.Recipients.Any() || m.SenderId == currentUserId || m.Recipients.Any(r => r.UserId == currentUserId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return history;
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
            .Include(m => m.Recipients)
            .Where(m => m.SenderId == userId || m.Recipients.Any(r => r.UserId == userId))
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();

        var details = new List<UserMessageDetailDto>();

        foreach (var m in messages)
        {
            details.Add(new UserMessageDetailDto
            {
                MessageId = m.Id,
                Direction = m.SenderId == userId ? "Sent" : "Received",
                OtherParties = m.SenderId == userId
                    ? string.Join(", ", m.Recipients.Select(r => r.UserId))
                    : m.SenderId ?? "Unknown",
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
}
