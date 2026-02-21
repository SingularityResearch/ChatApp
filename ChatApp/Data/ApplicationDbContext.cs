using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

/// <summary>
/// The main Entity Framework core database context for the application.
/// Provides access to identity tables and custom chat entities.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    /// <summary>
    /// Gets or sets the database set of chat messages.
    /// </summary>
    public DbSet<ChatMessage> ChatMessages { get; set; } = default!;

    /// <summary>
    /// Gets or sets the database set of chat recipients.
    /// </summary>
    public DbSet<ChatRecipient> ChatRecipients { get; set; } = default!;

    /// <summary>
    /// Gets or sets the database set of emoji reactions left on messages.
    /// </summary>
    public DbSet<MessageReaction> MessageReactions { get; set; } = default!;
}
