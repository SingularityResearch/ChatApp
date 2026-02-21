using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Services;

/// <summary>
/// Service responsible for tracking which users are currently online and determining their visibility to other users.
/// </summary>
public class ChatStateService
{
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatStateService"/> class.
    /// </summary>
    /// <param name="scopeFactory">A factory for creating dependency injection scopes.</param>
    public ChatStateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// Event triggered when a user's online state changes.
    /// </summary>
    public event Action? OnChange; // General state change

    // UserId -> UserName (Email)
    private ConcurrentDictionary<string, string> _onlineUsers = new();

    /// <summary>
    /// Marks a user as online.
    /// </summary>
    /// <param name="userId">The ID of the user logging in.</param>
    /// <param name="userName">The display name or username of the user.</param>
    public void SetUserOnline(string userId, string userName)
    {
        _onlineUsers.AddOrUpdate(userId, userName, (key, oldValue) => userName);
        NotifyStateChanged();
    }

    /// <summary>
    /// Marks a user as offline.
    /// </summary>
    /// <param name="userId">The ID of the user disconnecting.</param>
    public void SetUserOffline(string userId)
    {
        if (_onlineUsers.TryRemove(userId, out _))
        {
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Checks if a specified user is currently online.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user is online, otherwise false.</returns>
    public bool IsUserOnline(string userId)
    {
        return _onlineUsers.ContainsKey(userId);
    }

    /// <summary>
    /// Gets a dictionary of all currently online users.
    /// </summary>
    /// <returns>A dictionary mapping User ID to User Name.</returns>
    public Dictionary<string, string> GetOnlineUsers()
    {
        return _onlineUsers.ToDictionary(k => k.Key, v => v.Value);
    }

    /// <summary>
    /// Asynchronously fetches a list of users visible to the specified user based on role membership.
    /// Users can only see other users who share at least one role with them.
    /// </summary>
    /// <param name="userId">The ID of the current user.</param>
    /// <returns>A list of user data transfer objects representing visible users.</returns>
    public async Task<List<UserDto>> GetVisibleUsersAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return new List<UserDto>();

        var roles = await userManager.GetRolesAsync(user);

        // Strategy: If user has no roles, they see no one (or maybe only admins? let's stick to strict isolation)
        // Strictly: share a role.

        if (!roles.Any()) return new List<UserDto>();

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
                        Roles = userRoles.ToList()
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
