namespace PotionChanceEstimators;

public class SimpleEstimator : IEstimator
{
    private Belief _belief;
    
    public ref Belief Belief => ref _belief; 
    
    public SimpleEstimator()
    {
        _belief = default;
        _belief[4] = 1f; // Drop chance always starts at 40%
    }
    
    public SimpleEstimator(Belief b) => _belief = b;

    public void UpdateBelief(bool dropped, bool isElite, bool hasWhiteBeastStatue = false)
    {
        if (dropped)
        {
            // Shift all probability masses left
            for (int i = Belief.MinIndex + 1; i <= Belief.MaxIndex; i++)
            {
                _belief[i - 1] += _belief[i];
                _belief[i] = 0;
            }
        }
        else
        {
            // Shift all probability masses right
            for (int i = Belief.MaxIndex - 1; i >= Belief.MinIndex; i--)
            {
                _belief[i + 1] += _belief[i];
                _belief[i] = 0;
            }
        }
    }
    
    public float GetExpectedChance()
    {
        float expectedValue = 0f;
        
        // Note that we are only indexing the positive portion of Belief
        // because we assume any probability mass in the negative portion contributes zero potion chance
        for (int i = 0; i <= Belief.MaxIndex; i++)
        {
            expectedValue += _belief[i] * (i / 10f);
        }
        return expectedValue;
    }
}