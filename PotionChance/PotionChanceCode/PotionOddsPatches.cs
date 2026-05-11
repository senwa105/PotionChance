using HarmonyLib;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace PotionChance.PotionChanceCode;

public static class PotionOddsEvents
{
    public static event Action<bool, RoomType>? PotionRolled;
    public static event Action<float>? OddsOverridden;
    public static event Action? RunEnded;
    
    public static void InvokePotionRolled(bool dropped, RoomType roomType) => PotionRolled?.Invoke(dropped, roomType);
    public static void InvokeOddsOverridden(float newValue) => OddsOverridden?.Invoke(newValue);
    public static void InvokeRunEnded() => RunEnded?.Invoke();
}

[HarmonyPatch]
internal static class PotionOddsPatches
{
    [HarmonyPatch(typeof(PotionRewardOdds), nameof(PotionRewardOdds.Roll))]
    [HarmonyPostfix]
    static void RollPostfix(PotionRewardOdds __instance, bool __result, RoomType roomType)
    {
        PotionOddsEvents.InvokePotionRolled(__result, roomType);
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

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.OnEnded))]
    [HarmonyPostfix]
    static void OnEndedPostfix()
    {
        PotionOddsEvents.InvokeRunEnded();
    }
}