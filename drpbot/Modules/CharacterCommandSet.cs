using Discord;
using Discord.Interactions;
using drpbot.Data;
using drpbot.Objects;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace drpbot.Modules;

[Group("char", "Character commands")]
public class CharacterHandlerCommandSet : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;

    public CharacterHandlerCommandSet(InteractionService interactionService, IServiceProvider provider)
    {
        _interactionService = interactionService;
        _provider = provider;
    }

    // 
    // COMMON CHARACTER COMMANDS
    //
    [SlashCommand("card", "View a player's character card")]
    public async Task ViewCharacterCard(IUser user)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var characters = Db.Characters.Where(x => x.DiscordUserId == user.Id && x.Status == CharacterStatus.ACTIVE);
        if (characters.Count() > 0)
        {
            var x = new EmbedBuilder()
                .WithAuthor(user)
                .WithTitle($"Character card for {user.Username}")
                .WithColor(Color.Default)
                .AddField(new EmbedFieldBuilder().WithName("Name").WithValue(characters.First().Name))
                .AddField(new EmbedFieldBuilder().WithName("Race").WithValue(characters.First().Race))
                .AddField(new EmbedFieldBuilder().WithName("Description").WithValue(characters.First().Description))
                .Build();
            await RespondAsync(embed:x, ephemeral:true);
        }
        else
        {
            await RespondAsync("This player does not have a character card for you to view.", ephemeral:true);
        }
    }

    // Alias for ViewCharacterCard() command
    [SlashCommand("whois", "View a player's character card", ignoreGroupNames:true)]
    public async Task Whois(IUser user)
    {
        ViewCharacterCard(user);
    }

    //
    // CHARACTER CREATION COMMANDS
    //
    [SlashCommand("create", "Create a new character")]
    public async Task CreateNewCharacter(string newCharacterName, [Choice("Asami", "asami"), Choice("Urali", "urali"), Choice("Trixie", "trixie"), Choice("N'ji", "nji")] string race)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (Db.Characters.Where(x => x.DiscordUserId == Context.User.Id).Count() > 0)
        {
            await RespondAsync(
                "You already have an active character! You must retire your current character before creating a new one.", ephemeral:true);
        }
        else
        {
            var newCharacter = new PlayerCharacter()
            {
                Name = newCharacterName,
                Status = CharacterStatus.ACTIVE,
                Race = race,
                DiscordUserId = Context.User.Id,
                Description = "This user has not set a character description yet."
            };
            Db.Characters.Add(newCharacter);
            Db.SaveChanges();

            await RespondAsync(
                $"{newCharacterName} created! Be sure to set a character description with /char setdesc !", ephemeral:true);
        }
    }

    [SlashCommand("set-desc", "Set or update the description of your character")]
    public async Task ShowCharDescriptionModal()
    {
        await Context.Interaction.RespondWithModalAsync<CharDescriptionModal>("setCharDesc");
    } 
    
    // Modal handler for ShowCharDescriptionModal()
    [ModalInteraction("setCharDesc", ignoreGroupNames:true)]
    public async Task SetCharDescription(CharDescriptionModal modal)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            Db.Characters
                .First(x => x.DiscordUserId == Context.User.Id && x.Status == CharacterStatus.ACTIVE)
                .Description = modal.Description;
            Db.SaveChanges();
            await RespondAsync("Character description updated!", ephemeral: true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Couldn't update character description");
            await RespondAsync("Failed to update character description!", ephemeral:true);
        }
    }

    // TODO: implement verification button with interaction that auto-deletes
    [SlashCommand("retire", "Retire a character")]
    public async Task RetireCharacter()
    {
        await RespondAsync(
            "This feature is not yet implemented. For now, contact an ST if you want to retire a character", ephemeral:true);
    }

    public async Task ShowCharStatus()
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var charInfo = Db.Characters.FirstOrDefault(x => x.DiscordUserId == Context.User.Id);

        if (charInfo is null)
        {
            await RespondAsync("You don't have an active character!", ephemeral: true);
            return;
        }

        var resp = new EmbedBuilder().WithTitle("Your Status");

        resp.AddField(new EmbedFieldBuilder().WithName("Health")
            .WithValue($"{charInfo.CurrentHealth} / {charInfo.BaseHealth}").WithIsInline(true));
        // resp.AddField(new EmbedFieldBuilder().WithName("Vitality")
        //     .WithValue($"{charInfo.CurrentVitality} / {charInfo.BaseVitality}").WithIsInline(true));
        resp.AddField(new EmbedFieldBuilder().WithName("Status Ailments")
            .WithValue("No Status Ailments"));
        resp.AddField(new EmbedFieldBuilder().WithName("Attack")
            .WithValue(charInfo.BaseAttack).WithIsInline(true));
        resp.AddField(new EmbedFieldBuilder().WithName("Defense")
            .WithValue(charInfo.BaseDefense).WithIsInline(true));
        resp.AddField(new EmbedFieldBuilder().WithName("Speed")
            .WithValue(charInfo.BaseSpeed).WithIsInline(true));
    }

}

public class CharDescriptionModal : IModal
{
    public string Title => "Set Character Description";

    [InputLabel("Char Description")]
    [ModalTextInput("char_description", TextInputStyle.Paragraph, placeholder: "Set your character's new description here...")]
    public string Description { get; set; }
}