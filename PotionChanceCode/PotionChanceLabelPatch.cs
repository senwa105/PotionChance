using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;

namespace PotionChance.PotionChanceCode;

[HarmonyPatch]
public static class PotionChanceLabelPatch
{
    [HarmonyPatch(typeof(NPotionContainer), nameof(NPotionContainer.Initialize))]
    [HarmonyPostfix]
    public static void InitializePostfix(NPotionContainer __instance, IRunState runState, Control ____potionHolders)
    {
        if (____potionHolders.GetNodeOrNull<NPotionChanceContainer>("PotionChanceContainer") != null)
        {
            return;
        }

        PackedScene potionChanceScene = ResourceLoader.Load<PackedScene>(NPotionChanceContainer.ScenePath);
        NPotionChanceContainer container = potionChanceScene.Instantiate<NPotionChanceContainer>();

        ____potionHolders.AddChildSafely(container);
        ____potionHolders.MoveChild(container, 0);
    }

    [HarmonyPatch(typeof(NPotionContainer), "OnPotionShortcutPressed")]
    [HarmonyPrefix]
    public static bool OnPotionShortcutPressedPrefix(NPotionContainer __instance, List<NPotionHolder> ____holders)
    {
        Viewport? viewport = __instance.GetViewport();
        if (viewport != null && viewport.GuiGetFocusOwner() != null && NRun.Instance?.GlobalUi.TopBar.IsAncestorOf(viewport.GuiGetFocusOwner()) == true)
        {
            ActiveScreenContext.Instance.FocusOnDefaultControl();
        }
        else if (____holders.Count > 0)
        {
            ____holders[0].TryGrabFocus();
        }

        return false;
    }

    [HarmonyPatch(typeof(NTopBar), nameof(NTopBar.GetAllNavigableElements))]
    [HarmonyPostfix]
    public static void GetAllNavigableElementsPostfix(NTopBar __instance, List<Control> __result)
    {
        NPotionChanceContainer? chanceContainer = __instance.PotionContainer.GetNodeOrNull<NPotionChanceContainer>("MarginContainer/PotionHolders/PotionChanceContainer");
        if (chanceContainer == null || !chanceContainer.IsVisibleInTree() || __result.Contains(chanceContainer))
        {
            return;
        }

        if (__instance.PotionContainer.Holders.Count > 0)
        {
            int firstHolderIndex = __result.IndexOf(__instance.PotionContainer.Holders[0]);
            if (firstHolderIndex >= 0)
            {
                __result.Insert(firstHolderIndex, chanceContainer);
                return;
            }
        }

        int goldIndex = __result.IndexOf(__instance.Gold);
        if (goldIndex >= 0)
        {
            __result.Insert(goldIndex + 1, chanceContainer);
            return;
        }

        int hpIndex = __result.IndexOf(__instance.Hp);
        if (hpIndex >= 0)
        {
            __result.Insert(hpIndex + 1, chanceContainer);
            return;
        }

        __result.Add(chanceContainer);
    }
}