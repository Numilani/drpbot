using System.Text.RegularExpressions;

namespace drpbot.Services;

public class DiceRollerService
{
    public static (int, List<int>) Roll(string rollFormula)
    {
        int diceSum = 0;
        List<int> diceRolls = new List<int>();
        
        Match RegexMatch = Regex.Match(rollFormula, @"^(\d+)d(\d+)([-+]?\d+)?$");
        int diceQty = Convert.ToInt32(RegexMatch.Groups[1].Value); 
        int diceType = Convert.ToInt32(RegexMatch.Groups[2].Value);
        int flatSum = (string.IsNullOrEmpty(RegexMatch.Groups[3].Value) ? 0 : Convert.ToInt32(RegexMatch.Groups[3].Value));

        if (diceQty <= 0 || diceType <= 0) // catch odd edge cases like 0d1 or 1d0 so we can give back a funny response
        {
            return (-99, new List<int>());
        }
        
        Random random = new Random();
        for (int i = 0; i < diceQty; i++)
        {
            int roll = random.Next(1, diceType + 1);
            diceSum += roll;
            diceRolls.Add(roll);
        }
        diceRolls.Sort();
        return (diceSum + flatSum, diceRolls);
    }
    
    public static int GetMaxRoll(string rollFormula)
    {
        int diceSum = 0;
        
        Match RegexMatch = Regex.Match(rollFormula, @"^(\d+)d(\d+)([-+]?\d+)?$");
        int diceQty = Convert.ToInt32(RegexMatch.Groups[1].Value); 
        int diceType = Convert.ToInt32(RegexMatch.Groups[2].Value);
        int flatSum = (string.IsNullOrEmpty(RegexMatch.Groups[3].Value) ? 0 : Convert.ToInt32(RegexMatch.Groups[3].Value));

        if (diceQty <= 0 || diceType <= 0) // catch odd edge cases like 0d1 or 1d0 so we can give back a funny response
        {
            return -99;
        }
        
        for (int i = 0; i < diceQty; i++)
        {
            int roll = diceType;
            diceSum += roll;
        }
        return diceSum + flatSum;
    }
    
    public static int GetMinRoll(string rollFormula)
    {
        int diceSum = 0;
        
        Match RegexMatch = Regex.Match(rollFormula, @"^(\d+)d(\d+)([-+]?\d+)?$");
        int diceQty = Convert.ToInt32(RegexMatch.Groups[1].Value); 
        int diceType = Convert.ToInt32(RegexMatch.Groups[2].Value);
        int flatSum = (string.IsNullOrEmpty(RegexMatch.Groups[3].Value) ? 0 : Convert.ToInt32(RegexMatch.Groups[3].Value));

        if (diceQty <= 0 || diceType <= 0) // catch odd edge cases like 0d1 or 1d0 so we can give back a funny response
        {
            return (-99);
        }
        
        for (int i = 0; i < diceQty; i++)
        {
            int roll = 1;
            diceSum += roll;
        }
        return diceSum + flatSum;
    }
    
    
}