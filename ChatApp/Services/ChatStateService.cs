using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
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
    public event Action<string, string, string, DateTime, string?, bool, List<string>?>? OnMessageReceived;

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

    // This method simulates the Hub's SendMessage broadcasting
    public async void BroadcastMessage(string senderId, string senderName, string message, List<string> recipientIds, string? attachmentUrl)
    {
        var timestamp = DateTime.Now;

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
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving message: {ex.Message}");
        }

        // Invoke event for all listeners. The listeners (Chat.razor) will filter if it's for them.
        OnMessageReceived?.Invoke(senderId, senderName, message, timestamp, attachmentUrl, false, recipientIds);
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
