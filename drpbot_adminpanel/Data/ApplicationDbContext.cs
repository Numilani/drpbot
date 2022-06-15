using drpbot_adminpanel.Objects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace drpbot_adminpanel.Data;

public class ApplicationDbContext : IdentityDbContext<DiscordIdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}