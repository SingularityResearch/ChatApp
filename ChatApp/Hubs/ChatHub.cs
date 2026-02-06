using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Services;

namespace ChatApp.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatStateService _chatStateService;

    // Key: UserId, Value: List of ConnectionIds
    private static readonly ConcurrentDictionary<string, List<string>> _userConnections = new();

    public ChatHub(ChatStateService chatStateService)
    {
        _chatStateService = chatStateService;
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

    public async Task SendMessage(string message, List<string> recipientIds, string? attachmentUrl = null)
    {
        var senderId = Context.UserIdentifier;
        var timestamp = DateTime.Now;

        // If no recipients specified, broadcast logic (public chat) could go here.
        // But for this requirement, we assume either explicit selection or broadcast if empty.

        if (recipientIds == null || recipientIds.Count == 0)
        {
            await Clients.All.SendAsync("ReceiveMessage", senderId, message, timestamp, attachmentUrl, false);
        }
        else
        {
            // Send to sender 
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message, timestamp, attachmentUrl, true);

            // Send to recipients
            foreach (var recipientId in recipientIds)
            {
                if (_userConnections.TryGetValue(recipientId, out var connections))
                {
                    await Clients.Users(recipientId).SendAsync("ReceiveMessage", senderId, message, timestamp, attachmentUrl, true);
                }
            }
        }
    }
}
