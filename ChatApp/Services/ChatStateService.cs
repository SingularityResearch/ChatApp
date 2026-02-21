using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;
using ChatApp.Models;

namespace ChatApp.Services;

public class ChatStateService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatStateService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public event Action? OnChange; // General state change

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

    private void NotifyStateChanged() => OnChange?.Invoke();
}
