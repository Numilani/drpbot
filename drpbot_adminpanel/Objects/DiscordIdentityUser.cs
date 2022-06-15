using Microsoft.AspNetCore.Identity;

namespace drpbot_adminpanel.Objects;

public class DiscordIdentityUser : IdentityUser
{
    public ulong DiscordId { get; set; }
}