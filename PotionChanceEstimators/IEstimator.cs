namespace PotionChanceEstimators;

public interface IEstimator
{
    ref Belief Belief { get; }
    
    void UpdateBelief(bool dropped, bool isElite, bool hasWhiteBeastStatue = false);
    
    float GetExpectedChance();
}