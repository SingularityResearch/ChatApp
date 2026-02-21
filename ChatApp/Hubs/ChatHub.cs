using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Services;

namespace ChatApp.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatStateService _chatStateService;
    private readonly IChatMessageService _chatMessageService;

    // Key: UserId, Value: List of ConnectionIds
    private static readonly ConcurrentDictionary<string, List<string>> _userConnections = new();

    public ChatHub(ChatStateService chatStateService, IChatMessageService chatMessageService)
    {
        _chatStateService = chatStateService;
        _chatMessageService = chatMessageService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"[ChatHub] OnConnectedAsync: ConnectionId={Context.ConnectionId}, UserIdentifier={userId ?? "null"}");

        if (userId != null)
        {
            _userConnections.AddOrUpdate(userId,
                key => new List<string> { Context.ConnectionId },
                (key, list) =>
                {
                    lock (list)
                    {
                        list.Add(Context.ConnectionId);
                    }
                    return list;
                });

            var userName = Context.User?.Identity?.Name ?? "Unknown";
            _chatStateService.SetUserOnline(userId, userName);
            Console.WriteLine($"[ChatHub] User Online: {userId}. Broadcasting to Others.");

            // Notify others that this user is online
            await Clients.Others.SendAsync("UserConnected", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            if (_userConnections.TryGetValue(userId, out var list))
            {
                lock (list)
                {
                    list.Remove(Context.ConnectionId);
                    if (list.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                        _chatStateService.SetUserOffline(userId);
                        // Notify others offline
                        _ = Clients.Others.SendAsync("UserDisconnected", userId);
                    }
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string senderName, string message, List<string> recipientIds, string? attachmentUrl = null)
    {
        var senderId = Context.UserIdentifier;
        if (senderId == null) return;

        // Cannot send a message with no recipients (Global chat is disabled)
        if (recipientIds == null || recipientIds.Count == 0) return;

        var timestamp = DateTime.Now;

        // Save to DB using the new service
        int messageId = await _chatMessageService.SaveMessageAsync(senderId, senderName, message, recipientIds, attachmentUrl);

        // Send to sender 
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, senderName, message, timestamp, attachmentUrl, true, recipientIds, messageId);

        // Send to recipients
        foreach (var recipientId in recipientIds)
        {
            if (_userConnections.TryGetValue(recipientId, out var connections))
            {
                await Clients.Users(recipientId).SendAsync("ReceiveMessage", senderId, senderName, message, timestamp, attachmentUrl, true, recipientIds, messageId);
            }
        }
    }

    public async Task EditMessage(int messageId, string newContent)
    {
        bool success = await _chatMessageService.EditMessageAsync(messageId, newContent);

        if (success)
        {
            await Clients.All.SendAsync("MessageEdited", messageId, newContent);
        }
    }

    public async Task DeleteMessage(int messageId)
    {
        bool success = await _chatMessageService.DeleteMessageAsync(messageId);

        if (success)
        {
            await Clients.All.SendAsync("MessageDeleted", messageId);
        }
    }
}
