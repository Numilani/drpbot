using drpbot.Objects;
using Microsoft.EntityFrameworkCore;

namespace drpbot.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<SavedImpersonation> Impersonations { get; set; }
    public DbSet<PlayerCharacter> Characters { get; set; }
    
    // Everything below this line is for "config tables",
    // i.e. things that are really just db-stored option lists.
    // public DbSet<RacialOption> Races { get; set; }

    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

}