using System.Globalization;
using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using drpbot.Data;
using drpbot.Objects;
using drpbot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace drpbot.Modules;

public class ImpersonationCommandSet : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;
    public ImpersonationCommandSet(InteractionService interactionService, IServiceProvider provider)
    {
        _interactionService = interactionService;
        _provider = provider;
    }

    [SlashCommand("sayas", "Speak as an NPC")]
    public async Task SayAs(string npcName, string text)
    {
        var channel = Context.Channel as SocketTextChannel;
        try
        {
            var Webhook = await GetChannelWebhook(channel);
            
            using IServiceScope scope = _provider.CreateScope();
            ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (Db.Impersonations.Where(x => x.name == npcName).Count() > 0)
            {
                var imgUrl = Db.Impersonations.First(x => x.name == npcName).imgurl;
                Webhook.SendMessageAsync(text, avatarUrl: imgUrl, username: CultureInfo.CurrentCulture.TextInfo.ToTitleCase(npcName.ToLower()));
            }
            else
            {
                Webhook.SendMessageAsync(text, username: CultureInfo.CurrentCulture.TextInfo.ToTitleCase(npcName.ToLower()));
            }
            
            await RespondAsync("done");
            await DeleteOriginalResponseAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Couldn't execute SayAs command");
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }
    }

    [SlashCommand("asay", "Send a message as \"Ambient\" (descriptive text)")]
    public async Task AmbientSay(string text)
    {
        SayAs("ambient", $"*{text}*");
    }

    [SlashCommand("save-npc", "Save an impersionation as a preset NPC with custom PFP")]
    public async Task SaveImpersonation(string npcName, string imgUrl)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            if (StaticStringUtils.isValidUrl(imgUrl) &&
                Db.Impersonations.Where(x => x.name.ToLower() == npcName).Count() == 0)
            {
                Db.Impersonations.Add(new SavedImpersonation() { name = npcName, imgurl = imgUrl });
                Db.SaveChanges();
                await RespondAsync($"{npcName} is now saved as a standard impersonation!", ephemeral: true);
            }
            else
            {
                await RespondAsync(
                    "Couldn't save that NPC - check the name and ensure you have a valid URL for the image.");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Couldn't execute save-npc command");
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }
        
    }

    private static async Task<DiscordWebhookClient> GetChannelWebhook(SocketTextChannel? channel)
    {
        DiscordWebhookClient Webhook;
        if (channel.GetWebhooksAsync().Result.Where(x => x.Name.StartsWith("NPC_")).Count() == 0)
        {
            var x = await channel.CreateWebhookAsync(
                $"NPC_{channel.Name.ToLower()}_{DateTime.Now.ToString("MMddyyHHmmssfff")}");
            Webhook = new DiscordWebhookClient(x);
        }
        else
        {
            Webhook = new DiscordWebhookClient(channel.GetWebhooksAsync().Result.First(x => x.Name.StartsWith("NPC_")));
        }

        return Webhook;
    }
}