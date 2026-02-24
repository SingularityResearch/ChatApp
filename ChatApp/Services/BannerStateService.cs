using System.Collections.Concurrent;

namespace ChatApp.Services;

/// <summary>
/// Service coordinating the active chat roles between the Chat page and the Layout Banner.
/// </summary>
public class BannerStateService
{
    public event Action<string>? OnUserStateChange;

    private readonly ConcurrentDictionary<string, List<string>> _userActiveRoles = new();

    /// <summary>
    /// Gets the currently active roles for a specific user.
    /// If null or empty, the UI should fall back to the current user's default roles.
    /// </summary>
    public List<string>? GetActiveRoles(string userId)
    {
        if (_userActiveRoles.TryGetValue(userId, out var roles))
        {
            return roles;
        }
        return null;
    }

    /// <summary>
    /// Sets the currently active roles for a user and notifies subscribers.
    /// </summary>
    public void SetActiveRoles(string userId, IEnumerable<string> roles)
    {
        _userActiveRoles.AddOrUpdate(userId, roles.ToList(), (_, _) => roles.ToList());
        NotifyStateChanged(userId);
    }

    /// <summary>
    /// Clears the active roles for a user, defaulting back to the user's base roles.
    /// </summary>
    public void ClearActiveRoles(string userId)
    {
        if (_userActiveRoles.TryRemove(userId, out _))
        {
            NotifyStateChanged(userId);
        }
    }

    private void NotifyStateChanged(string userId) => OnUserStateChange?.Invoke(userId);
}
