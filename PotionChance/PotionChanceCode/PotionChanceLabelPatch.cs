using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Runs;

namespace PotionChance.PotionChanceCode;

[HarmonyPatch(typeof(NPotionContainer))]
public static class PotionChanceLabelPatch
{
    [HarmonyPatch(nameof(NPotionContainer.Initialize))]
    [HarmonyPostfix]
    public static void InitializePostfix(NPotionContainer __instance, IRunState runState, Control ____potionHolders)
    {
        PackedScene potionChanceScene = ResourceLoader.Load<PackedScene>(NPotionChanceContainer.ScenePath);
        NPotionChanceContainer container = potionChanceScene.Instantiate<NPotionChanceContainer>();
            
        ____potionHolders.AddChildSafely(container);
        ____potionHolders.MoveChild(container, 0);
    }
}