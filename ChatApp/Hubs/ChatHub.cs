using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChatApp.Services;

namespace ChatApp.Hubs;

/// <summary>
/// Hub for handling real-time chat communication and events via SignalR.
/// </summary>
/// <param name="chatStateService">Service to manage user online states.</param>
/// <param name="chatMessageService">Service to manage chat messages and reactions.</param>
[Authorize]
public class ChatHub(ChatStateService chatStateService, IChatMessageService chatMessageService) : Hub
{
    private readonly ChatStateService _chatStateService = chatStateService;
    private readonly IChatMessageService _chatMessageService = chatMessageService;

    // Key: UserId, Value: List of ConnectionIds
    private static readonly ConcurrentDictionary<string, List<string>> _userConnections = new();


    /// <summary>
    /// Called when a new connection is established with the hub.
    /// Tracks user connection and broadcasts online status to others.
    /// </summary>
    /// <returns>A task that represents the asynchronous connect operation.</returns>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"[ChatHub] OnConnectedAsync: ConnectionId={Context.ConnectionId}, UserIdentifier={userId ?? "null"}");

        if (userId != null)
        {
            _userConnections.AddOrUpdate(userId,
                key => [Context.ConnectionId],
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

    /// <summary>
    /// Called when a connection with the hub is terminated.
    /// Removes user connection tracking and broadcasts offline status if no connections remain.
    /// </summary>
    /// <param name="exception">An optional exception that occurred causing the disconnection.</param>
    /// <returns>A task that represents the asynchronous disconnect operation.</returns>
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

    /// <summary>
    /// Sends a message to a list of specified recipients.
    /// Saves the message to the database and broadcasts to connected clients.
    /// </summary>
    /// <param name="message">The text content of the message.</param>
    /// <param name="recipientIds">A list of user IDs to receive the message.</param>
    /// <param name="attachmentUrl">An optional URL to an attached file.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public async Task SendMessage(string message, List<string> recipientIds, string? attachmentUrl = null)
    {
        var senderId = Context.UserIdentifier;
        if (senderId == null) return;

        // Cannot send a message with no recipients (Global chat is disabled)
        if (recipientIds == null || recipientIds.Count == 0) return;

        var timestamp = DateTime.Now;

        // Prevent Sender Spoofing
        var senderName = Context.User?.Identity?.Name ?? "Unknown";

        // Save to DB using the new service
        int messageId = await _chatMessageService.SaveMessageAsync(senderId, senderName, message, recipientIds, attachmentUrl);

        // Send to sender 
        await Clients.Caller.SendAsync("ReceiveMessage", senderId, senderName, message, timestamp, attachmentUrl, true, recipientIds, messageId);

        // Send to recipients
        foreach (var recipientId in recipientIds)
        {
            if (_userConnections.TryGetValue(recipientId, out _))
            {
                await Clients.Users(recipientId).SendAsync("ReceiveMessage", senderId, senderName, message, timestamp, attachmentUrl, true, recipientIds, messageId);
            }
        }
    }

    /// <summary>
    /// Edits an existing message if the caller is the sender.
    /// </summary>
    /// <param name="messageId">The unique ID of the message to edit.</param>
    /// <param name="newContent">The updated text content.</param>
    /// <returns>A task that represents the asynchronous edit operation.</returns>
    public async Task EditMessage(int messageId, string newContent)
    {
        var currentUserId = Context.UserIdentifier;
        if (currentUserId == null) return;

        bool success = await _chatMessageService.EditMessageAsync(messageId, currentUserId, newContent);

        if (success)
        {
            await Clients.All.SendAsync("MessageEdited", messageId, newContent);
        }
    }

    /// <summary>
    /// Deletes an existing message if the caller is the sender.
    /// </summary>
    /// <param name="messageId">The unique ID of the message to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async Task DeleteMessage(int messageId)
    {
        var currentUserId = Context.UserIdentifier;
        if (currentUserId == null) return;

        bool success = await _chatMessageService.DeleteMessageAsync(messageId, currentUserId);

        if (success)
        {
            await Clients.All.SendAsync("MessageDeleted", messageId);
        }
    }

    /// <summary>
    /// Adds an emoji reaction to a specific message.
    /// </summary>
    /// <param name="messageId">The unique ID of the message.</param>
    /// <param name="emoji">The shortcode string representing the emoji.</param>
    /// <returns>A task that represents the asynchronous add reaction operation.</returns>
    public async Task AddReaction(int messageId, string emoji)
    {
        var currentUserId = Context.UserIdentifier;
        if (currentUserId == null) return;
        var userName = Context.User?.Identity?.Name ?? "User";

        bool success = await _chatMessageService.AddReactionAsync(messageId, currentUserId, emoji);
        if (success)
        {
            await Clients.All.SendAsync("ReactionAdded", messageId, currentUserId, userName, emoji);
        }
    }

    /// <summary>
    /// Removes a previously added emoji reaction from a message.
    /// </summary>
    /// <param name="messageId">The unique ID of the message.</param>
    /// <param name="emoji">The shortcode string representing the emoji.</param>
    /// <returns>A task that represents the asynchronous remove reaction operation.</returns>
    public async Task RemoveReaction(int messageId, string emoji)
    {
        var currentUserId = Context.UserIdentifier;
        if (currentUserId == null) return;

        bool success = await _chatMessageService.RemoveReactionAsync(messageId, currentUserId, emoji);
        if (success)
        {
            await Clients.All.SendAsync("ReactionRemoved", messageId, currentUserId, emoji);
        }
    }
}
