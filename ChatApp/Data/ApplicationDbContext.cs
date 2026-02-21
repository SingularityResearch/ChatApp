using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ChatMessage> ChatMessages { get; set; } = default!;
    public DbSet<ChatRecipient> ChatRecipients { get; set; } = default!;
    public DbSet<MessageReaction> MessageReactions { get; set; } = default!;
}
