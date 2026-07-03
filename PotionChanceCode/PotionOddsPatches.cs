using HarmonyLib;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;

namespace PotionChance.PotionChanceCode;

public static class PotionOddsEvents
{
    public static event Action<float, RoomType>? PotionRolled;
    public static event Action<float>? OddsOverridden;
    
    public static void InvokePotionRolled(float newOdds, RoomType roomType) => PotionRolled?.Invoke(newOdds, roomType);
    public static void InvokeOddsOverridden(float newOdds) => OddsOverridden?.Invoke(newOdds);
}

[HarmonyPatch]
internal static class PotionOddsPatches
{
    [HarmonyPatch(typeof(PotionRewardOdds), nameof(PotionRewardOdds.Roll))]
    [HarmonyPostfix]
    static void RollPostfix(PotionRewardOdds __instance, RoomType roomType)
    {
        PotionOddsEvents.InvokePotionRolled(__instance.CurrentValue, roomType);
    }

    [HarmonyPatch(typeof(AbstractOdds), nameof(AbstractOdds.OverrideCurrentValue))]
    [HarmonyPostfix]
    static void OverrideCurrentValuePostfix(AbstractOdds __instance)
    {
        if (__instance is PotionRewardOdds)
        {
            PotionOddsEvents.InvokeOddsOverridden(__instance.CurrentValue);
        }
    }
}