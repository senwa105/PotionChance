using BaseLib.Config;
using PotionChanceEstimators;

namespace PotionChance.PotionChanceCode;

[ConfigHoverTipsByDefault]
internal class PotionChanceConfig : SimpleModConfig
{
    public static bool ShowTrueChance { get; set; } = false;
    
    public enum EstimatorType { HMM, Sts1 }
    public static EstimatorType Estimator { get; set; } = EstimatorType.HMM;

    [ConfigHideInUI] 
    public static RunSaveData RunSaveSingleplayer { get; set; } = new();

    [ConfigHideInUI] public static RunSaveData RunSaveMultiplayer { get; set; } = new();
}

internal class RunSaveData
{
    public string Seed { get; set; } = string.Empty;
    
    public int LastSavedFloor { get; set; } = 0;
    
    public Belief?[] BeliefHistory { get; set; } = new Belief?[50];

    public void Clear()
    {
        Seed = string.Empty;
        LastSavedFloor = 0;
        Array.Clear(BeliefHistory, 0, BeliefHistory.Length);
    }
}