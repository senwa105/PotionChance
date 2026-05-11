using Xunit.Abstractions; 

namespace PotionChanceEstimators.Tests;

public class HmmEstimatorTests
{
    private const int Precision = 4;
    
    private readonly ITestOutputHelper _output;
    
    public HmmEstimatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitialBelief_ShouldBe40Percent()
    {
        var hmm = new HmmEstimator();
        
        float expectedChance = 0.4f;
        Assert.Equal(expectedChance, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[4], Precision);
    }

    [Fact]
    public void UpdateBelief_NormalPotion_ShouldShiftLeft()
    {
        var hmm = new HmmEstimator();
        
        hmm.UpdateBelief(isElite: false, dropped: true);
        
        // 40% -> 30%
        Assert.Equal(0.3f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[3], Precision);
    }

    [Fact]
    public void UpdateBelief_NormalNoPotion_ShouldShiftRight()
    {
        var hmm = new HmmEstimator();
        
        hmm.UpdateBelief(isElite: false, dropped: false);
        
        // 40% -> 50%
        Assert.Equal(0.5f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[5], Precision);
    }

    [Fact]
    public void UpdateBelief_ElitePotion_ShouldDiverge()
    {
        var hmm = new HmmEstimator();
        
        hmm.UpdateBelief(isElite: true, dropped: true);
        
        Assert.Equal(0.3476f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(0.7619f, hmm.Belief[3], Precision);
        Assert.Equal(0.2381f, hmm.Belief[5], Precision);
    }

    [Fact]
    public void UpdateBelief_EliteNoPotionPossible90_ShouldConverge()
    {
        var hmm = new HmmEstimator();
        
        hmm.UpdateBelief(isElite: true, dropped: true); // Chance for 50%
        hmm.UpdateBelief(isElite: false, dropped: false);
        hmm.UpdateBelief(isElite: false, dropped: false);
        hmm.UpdateBelief(isElite: false, dropped: false); 
        hmm.UpdateBelief(isElite: false, dropped: false); // Chance for 90%
        hmm.UpdateBelief(isElite: true, dropped: false);
        
        Assert.Equal(0.8f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[8], Precision);
    }

    [Fact]
    public void UpdateBelief_ElitePotion_EliteNoPotion_ShouldShiftRight()
    {
        var hmm = new HmmEstimator();
        
        hmm.UpdateBelief(isElite: true, dropped: true);
        hmm.UpdateBelief(isElite: true, dropped: false);
        
        Assert.Equal(0.4339f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(0.8307f, hmm.Belief[4], Precision);
        Assert.Equal(0.1693f, hmm.Belief[6], Precision);
    }

    [Fact]
    public void ZeroPercent_ElitePotion_ShouldShiftRight()
    {
        Belief b = default;
        b[0] = 1f;
        var hmm = new HmmEstimator(b);

        hmm.UpdateBelief(isElite: true, dropped: true);

        Assert.Equal(0.1f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[1], Precision);
    }

    [Fact]
    public void ZeroPercent_NormalNoPotion_ShouldShiftRight()
    {
        Belief b = default;
        b[0] = 1f;
        var hmm = new HmmEstimator(b);

        hmm.UpdateBelief(isElite: false, dropped: false);

        Assert.Equal(0.1f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[1], Precision);
    }

    [Fact]
    public void HundredPercent_PotionDropped_ShouldShiftLeft()
    {
        Belief b = default;
        b[10] = 1f;
        var hmm = new HmmEstimator(b);

        hmm.UpdateBelief(isElite: false, dropped: true);

        Assert.Equal(0.9f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[9], Precision);
    }

    [Fact]
    public void WhiteBeastStatue_PotionDropped_ShouldShiftLeft()
    {
        var hmm = new HmmEstimator();

        hmm.UpdateBelief(isElite: false, dropped: true, hasWhiteBeastStatue: true);

        Assert.Equal(0.3f, hmm.GetExpectedChance(), Precision);
        Assert.Equal(1f, hmm.Belief[3], Precision);
    }
}