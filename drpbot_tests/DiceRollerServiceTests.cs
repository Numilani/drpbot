using drpbot.Services;
using NUnit.Framework;

namespace drpbot_tests;

public class DiceRollerServiceTests
{
    [Test]
    [TestCase("3d6")]
    [TestCase("999d100")]
    [TestCase("1d1")]
    [TestCase("2d20+3")]
    [TestCase("10d20-6")]
    public void Test_Valid_Rolls(string rollFormula)
    {
        Assert.DoesNotThrow(() => { DiceRollerService.Roll(rollFormula);});
    }

    [Test]
    [TestCase("0d1")]
    [TestCase("1d0")]
    public void Test_Funny_Edgecase_Rolls(string rollFormula)
    {
        Assert.AreEqual(-99, DiceRollerService.Roll(rollFormula).Item1);
    }

    [Test]
    [TestCase("d")]
    [TestCase("100d")]
    [TestCase("d20")]
    [TestCase("10")]
    [TestCase("2d-6")]
    [TestCase("-5d20")]
    public void Test_Invalid_Rolls(string rollFormula)
    {
        Assert.Catch(() => { DiceRollerService.Roll(rollFormula); });
        
    }
}