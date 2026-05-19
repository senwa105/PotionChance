namespace EstimatorComparison;

public class PotionSimulator(int seed)
{
    public float CurrentChance = 0.4f;
    
    private readonly Random _rng = new(seed);

    public bool Roll(bool isElite, bool hasWhiteBeastStatue = false)
    {
        float startingChance = CurrentChance;
        
        float num = (float)_rng.NextDouble();
        float change = num < startingChance || hasWhiteBeastStatue ? -0.1f : 0.1f;
        CurrentChance += change;

        if (hasWhiteBeastStatue) return true;
        
        float eliteBonus = isElite ? 0.125f : 0f;
        return num < startingChance + eliteBonus;
    }
}
