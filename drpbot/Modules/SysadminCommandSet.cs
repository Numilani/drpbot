using System.Data.Common;
using System.Reflection;
using Discord;
using Discord.Interactions;
using drpbot.Data;
using drpbot.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace drpbot.Modules;

[Group("sys", "Sysadmin commands")]
public class SysadminCommandSet : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _provider;

    public SysadminCommandSet(InteractionService interactionService, IServiceProvider provider)
    {
        _interactionService = interactionService;
        _provider = provider;
    }

    [SlashCommand("item-give", "Give the user an item")]
    public async Task GiveItemSuper(IUser user, string item)
    {
        try
        {
            int id = Convert.ToInt32(item);
            GiveItem(user, id);
        }
        catch
        {
            GiveItem(user, item);
        }
    }
    
    public async Task GiveItem(IUser user, string itemName)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var items = Db.Items.Where(x => x.Name == itemName).ToList();
        if (items.Count > 1)
        {
            var response = new EmbedBuilder()
                .WithTitle($"Multiple Items with name {itemName}");
            foreach (var possibleItem in items)
            {
                response.AddField(new EmbedFieldBuilder().WithName(possibleItem.Id.ToString()).WithValue(possibleItem.Name).WithIsInline(true));
            }

            await RespondAsync(embed: response.Build(), ephemeral: true);
        }
        else
        {
            var newInvItem = new InventoryObject()
            {
                OwnerId = user.Id,
                OwnerType = OwnerType.PLAYER,
                UnderlyingItem = items[0]
            };

            try
            {
                Db.InWorldItems.Add(newInvItem);
                Db.SaveChanges();
                await RespondAsync($"Gave new item to {user.Username}: Id# {newInvItem.Id}", ephemeral: true);
            }
            catch (Exception ex)
            {
                await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);

            }
        }
    }

    public async Task GiveItem(IUser user, int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = Db.Items.Find(itemId);
        
        var newInvItem = new InventoryObject()
        {
            OwnerId = user.Id,
            OwnerType = OwnerType.PLAYER,
            UnderlyingItem = item
        };

        try
        {
            Db.InWorldItems.Add(newInvItem);
            Db.SaveChanges();
            
            await RespondAsync($"Gave new item to {user.Username}: Id# {newInvItem.Id}", ephemeral: true);
        }
        catch (Exception ex)
        {
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }

    }

    [SlashCommand("item-erase", "Permanently erase an item from a user's inventory")]
    public async Task RemoveItem(IUser user, int itemId)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = Db.InWorldItems.Include(x => x.UnderlyingItem).FirstOrDefault(x => x.Id == itemId);

        try
        {
            Db.InWorldItems.Remove(item);
            Db.SaveChanges();
            await RespondAsync($"Erased {user.Username}'s item from existence.");
        }
        catch (Exception ex)
        {
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }
        
    }

    [SlashCommand("item-add", "Create a new item")]
    public async Task MakeItem(string name, string description, [Autocomplete(typeof(ItemDisplayTypeAutocompleteHandler))] string primaryDisplayType, string secondaryDisplayType, double weight = 0.0)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var item = new Item()
        {
            Name = name,
            Description = description,
            DisplayedItemType = primaryDisplayType,
            DisplayedItemSecondaryType = secondaryDisplayType,
            Weight = weight
        };

        try
        {
            Db.Items.Add(item);
            Db.SaveChanges();

            await RespondAsync($"Saved new item: ItemId {item.Id}", ephemeral: true);
        }
        catch (Exception ex)
        {
            await RespondAsync($"!! Ran into an issue: {ex.Message}", ephemeral:true);
        }
    }

    [SlashCommand("invsee", "View a player's inventory")]
    public async Task ViewInventory(IUser user)
    {
        using IServiceScope scope = _provider.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invItems = Db.InWorldItems.Include(x => x.UnderlyingItem).Where(x => x.OwnerId == user.Id).ToList();

        string invListString = "";
        foreach (var item in invItems)
        {
            invListString += $"{item.Name}\n\n";
        }

        var resp = new EmbedBuilder().WithTitle($"Inventory of {user.Username}");
        resp.AddField(new EmbedFieldBuilder().WithName("Inventory").WithValue(invListString));
        
        await RespondAsync(embed: resp.Build(), ephemeral: true);
    }

    [SlashCommand("version", "See version info regarding the bot")]
    public async Task ViewVersionInfo()
    {
        var resp = new EmbedBuilder().WithTitle("Version Info");
        resp.AddField(new EmbedFieldBuilder().WithName("Version #")
            .WithValue(Assembly.GetExecutingAssembly().GetName().Version));
        resp.AddField(new EmbedFieldBuilder().WithName("Details").WithValue("Developed by Numilani"));
        await RespondAsync(embed: resp.Build(), ephemeral:true);
    }
}

public class ItemDisplayTypeAutocompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter, IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        ApplicationDbContext Db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var suggestions = new List<AutocompleteResult>()
        {
            new AutocompleteResult("Melee Weapon", "Melee Weapon"),
            new AutocompleteResult("Ranged Weapon", "Ranged Weapon"),
            new AutocompleteResult("Shield", "Shield"),
            new AutocompleteResult("Tool", "Tool"),
            new AutocompleteResult("Armor", "Armor"),
            new AutocompleteResult("Apparel", "Apparel"),
            new AutocompleteResult("Trinket", "Trinket"),
            new AutocompleteResult("Consumable", "Consumable"),
            new AutocompleteResult("Artifact", "Artifact"),
            new AutocompleteResult("Unique Item", "Unique Item"),
            new AutocompleteResult("???", "???"),
        };
        return AutocompletionResult.FromSuccess(suggestions.Take(25));
    }
}