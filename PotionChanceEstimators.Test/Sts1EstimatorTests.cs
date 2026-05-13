using Xunit.Abstractions; 

namespace PotionChanceEstimators.Tests;

public class Sts1EstimatorTests
{
    private const int Precision = 4;
    
    private readonly ITestOutputHelper _output;
    
    public Sts1EstimatorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Constructor_InitialState_ShouldBe40Percent()
    {
        var estimator = new Sts1Estimator();
        
        Assert.Equal(0.4f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[4], Precision);
    }

    [Fact]
    public void UpdateBelief_NormalRoom_PotionDropped_ShouldShiftLeft()
    {
        var estimator = new Sts1Estimator();
        
        estimator.UpdateBelief(dropped: true, isElite: false);
        
        Assert.Equal(0.3f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[3], Precision);
    }

    [Fact]
    public void UpdateBelief_NormalRoom_NoPotion_ShouldShiftRight()
    {
        var estimator = new Sts1Estimator();
        
        estimator.UpdateBelief(dropped: false, isElite: false);
        
        Assert.Equal(0.5f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[5], Precision);
    }

    [Fact]
    public void UpdateBelief_EliteRoom_ShouldIgnoreEliteBonus()
    {
        var estimator = new Sts1Estimator();
        
        estimator.UpdateBelief(dropped: true, isElite: true);
        
        Assert.Equal(0.3f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[3], Precision);
    }

    [Fact]
    public void Boundary_MaxIndex_ShouldNotExceedHundredPercent()
    {
        Belief b = default;
        b[Belief.MaxIndex] = 1f;
        var estimator = new Sts1Estimator(b);

        estimator.UpdateBelief(dropped: false, isElite: false);

        Assert.Equal(1.0f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[Belief.MaxIndex], Precision);
    }

    [Fact]
    public void Boundary_MinIndex_ShouldNotGoBelowNegativeLimit()
    {
        Belief b = default;
        b[Belief.MinIndex] = 1f;
        var estimator = new Sts1Estimator(b);

        estimator.UpdateBelief(dropped: true, isElite: false);
        
        Assert.Equal(0.0f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[Belief.MinIndex], Precision);
    }

    [Fact]
    public void NegativeState_NoPotion_ShouldShiftRightButReportZeroChance()
    {
        Belief b = default;
        b[-2] = 1f; // Belief is -20%
        var estimator = new Sts1Estimator(b);
        
        estimator.UpdateBelief(dropped: false, isElite: false);
        
        Assert.Equal(0.0f, estimator.GetExpectedChance(), Precision);
        Assert.Equal(1f, estimator.Belief[-1], Precision);
    }
}