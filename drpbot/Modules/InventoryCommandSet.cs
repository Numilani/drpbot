using Discord;
using Discord.Interactions;
using drpbot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace drpbot.Modules;

[Group("inv", "Inventory Commands")]
public class InventoryCommandSet : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;

    public InventoryCommandSet(InteractionService interactionService, IServiceProvider provider)
    {
        _interactionService = interactionService;
        _provider = provider;
    }

    [SlashCommand("see", "View your inventory")]
    public async Task ViewInventory()
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invItems = Db.InWorldItems.Include(x => x.UnderlyingItem).Where(x => x.OwnerId == Context.User.Id).ToList();

        string invListString = "";
        foreach (var item in invItems)
        {
            invListString += $"{item.Name}\n\n";
        }

        if (String.IsNullOrEmpty(invListString)) invListString = "Your inventory is empty!";

        var resp = new EmbedBuilder().WithTitle($"Your Inventory");
        resp.AddField(new EmbedFieldBuilder().WithName("Inventory").WithValue(invListString));
        
        await RespondAsync(embed: resp.Build(), ephemeral: true);
    }

    // Alias for /inv see
    [SlashCommand("i", "View your inventory", ignoreGroupNames:true)]
    public async Task ViewInventoryAlias()
    {
        ViewInventory();
    }

    [SlashCommand("examine", "Examine an item in your inventory")]
    public async Task ExamineInventoryItem([Autocomplete(typeof(InventoryAutocompleteHandler))] int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var item = Db.InWorldItems.Include(x => x.UnderlyingItem).FirstOrDefault(x => x.Id == itemId);

        if (item is null)
        {
            await RespondAsync("Couldn't find that item!", ephemeral: true);
            return;
        }

        var resp = new EmbedBuilder().WithTitle(item.Name);

        resp.AddField(new EmbedFieldBuilder().WithName("Description").WithValue(item.Description));
        resp.AddField(new EmbedFieldBuilder().WithName("Type")
            .WithValue($"{item.UnderlyingItem.DisplayedItemType} ({item.UnderlyingItem.DisplayedItemSecondaryType})"));

        await RespondAsync(embed: resp.Build(), ephemeral: true);
    }

    [SlashCommand("display", "Show off an item in chat")]
    public async Task ShowItem([Autocomplete(typeof(InventoryAutocompleteHandler))] int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var item = Db.InWorldItems.Include(x => x.UnderlyingItem).FirstOrDefault(x => x.Id == itemId);
        
        var character = Db.Characters.FirstOrDefault(x => x.DiscordUserId == Context.User.Id);
        
        if (item is null)
        {
            await RespondAsync("Couldn't find that item!", ephemeral: true);
            return;
        }

        var resp = new EmbedBuilder().WithTitle(item.Name);

        resp.AddField(new EmbedFieldBuilder().WithName("Description").WithValue(item.Description));
        resp.AddField(new EmbedFieldBuilder().WithName("Type")
            .WithValue($"{item.UnderlyingItem.DisplayedItemType} ({item.UnderlyingItem.DisplayedItemSecondaryType})"));

        await RespondAsync($"{character?.Name ?? Context.User.Username} displays an item: ", embed: resp.Build());
    }

    public async Task EquipItem(int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var item = Db.InWorldItems.Include(x => x.UnderlyingItem).FirstOrDefault(x => x.OwnerId == Context.User.Id);

        if (item is null)
        {
            await RespondAsync("Couldn't find that item!", ephemeral:true);
            return;
        }

        if (!item.UnderlyingItem.CustomFlags.ContainsKey("EquipmentSlot"))
        {
            await RespondAsync("You cannot equip a non-equippable item!", ephemeral: true);
            return;
        }
        
        if (Db.InWorldItems.Count(x => x.ItemState["EquippedInSlot"] == item.UnderlyingItem.CustomFlags["EquipmentSlot"]) > 0)
        {
            await RespondAsync("You must unequip what you're wearing in that slot first!", ephemeral: true);
            return;
        }

        try
        {
            item.ItemState.Add("EquippedInSlot", item.UnderlyingItem.CustomFlags["EquipmentSlot"]);
            Db.SaveChanges();
            await RespondAsync("Item Equipped!", ephemeral: true);
        }
        catch (Exception ex)
        {
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }

    }

    public async Task DequipItem(int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // TODO: finish implementation
    }
}

public class InventoryAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var invItems = Db.InWorldItems.Include(x => x.UnderlyingItem).Where(x => x.OwnerId == context.User.Id).ToList();
        var suggestions = new List<AutocompleteResult>();
        foreach (var item in invItems)
        {
            suggestions.Add(new AutocompleteResult(item.Name, item.Id));
        }

        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}