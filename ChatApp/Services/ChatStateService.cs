using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;

namespace ChatApp.Services;

public class ChatStateService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatStateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public event Action? OnChange; // General state change
    public event Action<string, string, string, DateTime, string?, bool, List<string>?, int>? OnMessageReceived;

    // UserId -> UserName (Email)
    private ConcurrentDictionary<string, string> _onlineUsers = new();

    public void SetUserOnline(string userId, string userName)
    {
        _onlineUsers.AddOrUpdate(userId, userName, (key, oldValue) => userName);
        NotifyStateChanged();
    }

    public void SetUserOffline(string userId)
    {
        if (_onlineUsers.TryRemove(userId, out _))
        {
            NotifyStateChanged();
        }
    }

    public bool IsUserOnline(string userId)
    {
        return _onlineUsers.ContainsKey(userId);
    }

    public Dictionary<string, string> GetOnlineUsers()
    {
        return _onlineUsers.ToDictionary(k => k.Key, v => v.Value);
    }

    public async Task<List<ApplicationUser>> GetVisibleUsersAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return new List<ApplicationUser>();

        var roles = await userManager.GetRolesAsync(user);

        // Strategy: If user has no roles, they see no one (or maybe only admins? let's stick to strict isolation)
        // Strictly: share a role.

        if (!roles.Any()) return new List<ApplicationUser>();

        var visibleUsers = new HashSet<string>();
        var results = new List<ApplicationUser>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role);
            foreach (var u in usersInRole)
            {
                if (u.Id != userId && visibleUsers.Add(u.Id))
                {
                    results.Add(u);
                }
            }
        }
        return results;
    }

    // This method simulates the Hub's SendMessage broadcasting
    public async void BroadcastMessage(string senderId, string senderName, string message, List<string> recipientIds, string? attachmentUrl)
    {
        var timestamp = DateTime.Now;
        int messageId = 0;

        // Save to DB
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var msgEntity = new ChatMessage
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    Message = message,
                    Timestamp = timestamp,
                    AttachmentUrl = attachmentUrl
                };

                if (recipientIds != null && recipientIds.Any())
                {
                    msgEntity.Recipients = recipientIds.Select(uid => new ChatRecipient { UserId = uid }).ToList();
                }

                db.ChatMessages.Add(msgEntity);
                await db.SaveChangesAsync();
                messageId = msgEntity.Id;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving message: {ex.Message}");
        }

        // Invoke event for all listeners. The listeners (Chat.razor) will filter if it's for them.
        OnMessageReceived?.Invoke(senderId, senderName, message, timestamp, attachmentUrl, false, recipientIds, messageId);
    }

    public event Action<int, string>? OnMessageEdited;
    public event Action<int>? OnMessageDeleted;

    public async Task EditMessage(int messageId, string newContent)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var msg = await db.ChatMessages.FindAsync(messageId);
                if (msg != null)
                {
                    msg.Message = newContent;
                    await db.SaveChangesAsync();
                    OnMessageEdited?.Invoke(messageId, newContent);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error editing message: {ex.Message}");
        }
    }

    public async Task DeleteMessage(int messageId)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var msg = await db.ChatMessages.FindAsync(messageId);
                if (msg != null)
                {
                    db.ChatMessages.Remove(msg);
                    await db.SaveChangesAsync();
                    OnMessageDeleted?.Invoke(messageId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting message: {ex.Message}");
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
