using HarmonyLib;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;

namespace PotionChance.PotionChanceCode;

public static class PotionOddsEvents
{
    public static event Action<RoomType>? PotionRolled;
    public static event Action? OddsOverridden;
    
    public static void InvokePotionRolled(RoomType roomType) => PotionRolled?.Invoke(roomType);
    public static void InvokeOddsOverridden() => OddsOverridden?.Invoke();
}

[HarmonyPatch]
internal static class PotionOddsPatches
{
    [HarmonyPatch(typeof(PotionRewardOdds), nameof(PotionRewardOdds.Roll))]
    [HarmonyPostfix]
    static void RollPostfix(RoomType roomType)
    {
        PotionOddsEvents.InvokePotionRolled(roomType);
    }

    [HarmonyPatch(typeof(AbstractOdds), nameof(AbstractOdds.OverrideCurrentValue))]
    [HarmonyPostfix]
    static void OverrideCurrentValuePostfix(AbstractOdds __instance)
    {
        if (__instance is PotionRewardOdds)
        {
            PotionOddsEvents.InvokeOddsOverridden();
        }
    }
}