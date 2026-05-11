using BaseLib.Config;
using PotionChanceEstimators;

namespace PotionChance.PotionChanceCode;

[ConfigHoverTipsByDefault]
internal class PotionChanceConfig : SimpleModConfig
{
    public static bool ShowTrueChance { get; set; } = false;
    
    public enum EstimatorType { HMM, Simple }
    public static EstimatorType Estimator { get; set; } = EstimatorType.HMM;
}
