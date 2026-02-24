using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Services;

/// <summary>
/// Service responsible for tracking which users are currently online and determining their visibility to other users.
/// </summary>
/// <param name="scopeFactory">A factory for creating dependency injection scopes.</param>
public class ChatStateService(IServiceScopeFactory scopeFactory)
{

    /// <summary>
    /// Event triggered when a user's online state changes.
    /// </summary>
    public event Action? OnChange; // General state change

    // UserId -> { ConnectionId -> UserName }
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> _userConnections = new();

    /// <summary>
    /// Adds a connection for a user, marking them as online if this is their first connection.
    /// </summary>
    /// <param name="userId">The ID of the user logging in.</param>
    /// <param name="userName">The display name or username of the user.</param>
    /// <param name="connectionId">A unique identifier for this connection session.</param>
    public void AddUserConnection(string userId, string userName, string connectionId)
    {
        var userConns = _userConnections.GetOrAdd(userId, _ => new ConcurrentDictionary<string, string>());
        bool isFirstConnection = userConns.IsEmpty;
        
        userConns.AddOrUpdate(connectionId, userName, (_, _) => userName);

        if (isFirstConnection)
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Removes a specific connection for a user. Marks them as offline if they have no remaining connections.
    /// </summary>
    /// <param name="userId">The ID of the user disconnecting.</param>
    /// <param name="connectionId">The unique identifier for the connection session ending.</param>
    public void RemoveUserConnection(string userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var userConns))
        {
            userConns.TryRemove(connectionId, out _);

            if (userConns.IsEmpty)
            {
                _userConnections.TryRemove(userId, out _);
                NotifyStateChanged();
            }
        }
    }

    /// <summary>
    /// Checks if a specified user is currently online (has at least one active connection).
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is online, otherwise false.</returns>
    public bool IsUserOnline(string userId)
    {
        return _userConnections.ContainsKey(userId) && !_userConnections[userId].IsEmpty;
    }

    /// <summary>
    /// Gets a dictionary of all currently online users and their display names.
    /// </summary>
    /// <returns>A dictionary mapping User ID to User Name.</returns>
    public Dictionary<string, string> GetOnlineUsers()
    {
        var onlineUsers = new Dictionary<string, string>();
        foreach (var kvp in _userConnections)
        {
            if (!kvp.Value.IsEmpty)
            {
                // Take the username from the first active connection
                var username = kvp.Value.Values.FirstOrDefault();
                if (username != null)
                {
                    onlineUsers[kvp.Key] = username;
                }
            }
        }
        return onlineUsers;
    }

    /// <summary>
    /// Asynchronously fetches a list of users visible to the specified user based on role membership.
    /// Users can only see other users who share at least one role with them.
    /// </summary>
    /// <param name="userId">The ID of the current user.</param>
    /// <returns>A list of user data transfer objects representing visible users.</returns>
    public async Task<List<UserDto>> GetVisibleUsersAsync(string userId)
    {
        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return [];

        var roles = await userManager.GetRolesAsync(user);

        // Strategy: If user has no roles, they see no one (or maybe only admins? let's stick to strict isolation)
        // Strictly: share a role.

        if (!roles.Any()) return [];

        var visibleUsers = new HashSet<string>();
        var results = new List<UserDto>();

        foreach (var role in roles)
        {
            var usersInRole = await userManager.GetUsersInRoleAsync(role);
            foreach (var u in usersInRole)
            {
                if (u.Id != userId && visibleUsers.Add(u.Id))
                {
                    var userRoles = await userManager.GetRolesAsync(u);
                    results.Add(new UserDto
                    {
                        Id = u.Id,
                        UserName = u.UserName ?? "Unknown",
                        Roles = [.. userRoles]
                    });
                }
            }
        }
        return results;
    }

    /// <summary>
    /// Invokes the <see cref="OnChange"/> event to notify subscribers of state updates.
    /// </summary>
    private void NotifyStateChanged() => OnChange?.Invoke();
}
