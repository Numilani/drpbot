using Discord.Interactions;
using drpbot.Services;
using Serilog;

namespace drpbot.Modules;

public class UtilityCommandSet : InteractionModuleBase<SocketInteractionContext>
{
    private readonly InteractionService _interactionService;

    public UtilityCommandSet(InteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [SlashCommand("roll", "Roll some dice")]
    public async Task Roll(string rollFormula = "3d6")
    {
        try
        {
            var (result, rolls) = DiceRollerService.Roll(rollFormula);
            if (result == -99) // catch "funny response" value from DRS.Roll()
            {
                await RespondAsync("Trying to trick the dice roller, eh? Joke's on you, I already thought of that! -_-");
            }
            else
            {
                await RespondAsync($"{Context.User.Username} rolled {result} [{string.Join(", ", rolls)}] ({rollFormula})");
            }
        }
        catch (Exception ex)
        {
            Log.Error("Roll command failed with input {fmla}", rollFormula);
            await RespondAsync("Roll failed!");
        }
    }
}