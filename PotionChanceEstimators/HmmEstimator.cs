namespace PotionChanceEstimators;

public class HmmEstimator : IEstimator
{ 
    private Belief _belief;
    
    public ref Belief Belief => ref _belief; 

    public HmmEstimator()
    {
        _belief = default;
        _belief[4] = 1f; // Drop chance always starts at 40%
    }

    public HmmEstimator(Belief b) => _belief = b;

    public void UpdateBelief(bool dropped, bool isElite, bool hasWhiteBeastStatue = false)
    {
        if (hasWhiteBeastStatue)
        {
            // Shift all probability masses left
            for (int i = Belief.MinIndex + 1; i <= Belief.MaxIndex; i++)
            {
                _belief[i - 1] += _belief[i];
                _belief[i] = 0;
            }

            return;
        }
        
        Belief tempBelief = default;
        
        // Weight prior by the likelihood of the observation (Bayes' Rule)
        for (int i = Belief.MinIndex; i <= Belief.MaxIndex; i++)
        {
            float baseRate = i / 10f;
            float eliteBonus = isElite ? 0.125f : 0f;
            float totalDropChance = Math.Clamp(baseRate + eliteBonus, 0f, 1f);
            float obsLikelihood = dropped ? totalDropChance : (1f - totalDropChance);

            tempBelief[i] = _belief[i] * obsLikelihood;
        }

        tempBelief.Normalize();
        
        // Shift probability masses (conditioning on base drop chance)
        _belief = default;

        for (int i = Belief.MinIndex; i <= Belief.MaxIndex; i++)
        {
            if (tempBelief[i] == 0) continue;

            float baseRate = i / 10f;
            float totalDropChanceGivenElite = Math.Clamp(baseRate + 0.125f, 0f, 1f);
            float pBaseDropTriggered = dropped
                ? (isElite ? baseRate / totalDropChanceGivenElite : 1f)
                : 0f;

            float massShiftingLeft = tempBelief[i] * pBaseDropTriggered;
            float massShiftingRight = tempBelief[i] * (1f - pBaseDropTriggered);

            int indexLeft = Math.Max(Belief.MinIndex, i - 1);
            int indexRight = Math.Min(Belief.MaxIndex, i + 1);

            _belief[indexLeft] += massShiftingLeft;
            _belief[indexRight] += massShiftingRight;
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