using ChatApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services;

/// <summary>
/// Service for managing role-based program banners.
/// </summary>
public class BannerService(IDbContextFactory<ApplicationDbContext> contextFactory)
{
    /// <summary>
    /// Gets all banners configured in the system.
    /// </summary>
    public async Task<List<ProgramBanner>> GetAllBannersAsync()
    {
        using var context = await contextFactory.CreateDbContextAsync();
        return await context.ProgramBanners.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Updates or creates a banner for a specific role.
    /// </summary>
    public async Task UpdateBannerAsync(ProgramBanner banner)
    {
        using var context = await contextFactory.CreateDbContextAsync();
        var existing = await context.ProgramBanners.FirstOrDefaultAsync(b => b.RoleName == banner.RoleName);
        if (existing != null)
        {
            existing.BannerText = banner.BannerText;
            existing.BackgroundColor = banner.BackgroundColor;
            existing.TextColor = banner.TextColor;
            existing.IsActive = banner.IsActive;
            context.ProgramBanners.Update(existing);
        }
        else
        {
            context.ProgramBanners.Add(banner);
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all active banners that match any of the provided roles.
    /// </summary>
    public async Task<List<ProgramBanner>> GetActiveBannersForRolesAsync(IEnumerable<string> roles)
    {
        if (roles == null || !roles.Any())
            return new List<ProgramBanner>();

        using var context = await contextFactory.CreateDbContextAsync();
        // Find all active banners that match the provided roles
        return await context.ProgramBanners
            .AsNoTracking()
            .Where(b => b.IsActive && roles.Contains(b.RoleName))
            .ToListAsync();
    }
}
