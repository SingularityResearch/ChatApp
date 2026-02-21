using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

public interface IChatMessageService
{
    Task<int> SaveMessageAsync(string senderId, string senderName, string message, List<string>? recipientIds, string? attachmentUrl);
    Task<bool> EditMessageAsync(int messageId, string currentUserId, string newContent);
    Task<bool> DeleteMessageAsync(int messageId, string currentUserId);
    Task<List<Data.ChatMessage>> GetMessageHistoryAsync(string currentUserId);
    Task<List<UserActivityDto>> GetUserActivityReportAsync();
    Task<List<UserMessageDetailDto>> GetUserMessageDetailsAsync(string userId);
}

public class ChatMessageService : IChatMessageService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatMessageService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<int> SaveMessageAsync(string senderId, string senderName, string message, List<string>? recipientIds, string? attachmentUrl)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var msgEntity = new Data.ChatMessage
        {
            SenderId = senderId,
            SenderName = senderName,
            Message = message,
            Timestamp = DateTime.Now,
            AttachmentUrl = attachmentUrl
        };

        if (recipientIds != null && recipientIds.Any())
        {
            msgEntity.Recipients = recipientIds.Select(uid => new ChatRecipient { UserId = uid }).ToList();
        }

        db.ChatMessages.Add(msgEntity);
        await db.SaveChangesAsync();
        return msgEntity.Id;
    }

    public async Task<bool> EditMessageAsync(int messageId, string currentUserId, string newContent)
    {
        using var scope = _scopeFactory.CreateScope();
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

    public async Task<bool> DeleteMessageAsync(int messageId, string currentUserId)
    {
        using var scope = _scopeFactory.CreateScope();
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

    public async Task<List<Data.ChatMessage>> GetMessageHistoryAsync(string currentUserId)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var history = await db.ChatMessages
            .Include(m => m.Recipients)
            .Where(m => !m.Recipients.Any() || m.SenderId == currentUserId || m.Recipients.Any(r => r.UserId == currentUserId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return history;
    }

    public async Task<List<UserActivityDto>> GetUserActivityReportAsync()
    {
        using var scope = _scopeFactory.CreateScope();
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

    public async Task<List<UserMessageDetailDto>> GetUserMessageDetailsAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
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
