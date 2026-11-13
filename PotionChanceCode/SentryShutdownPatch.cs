using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Debug;

namespace PotionChance.PotionChanceCode;

[HarmonyPatch(typeof(SentryService), nameof(SentryService.Shutdown))]
internal static class SentryShutdownPatch
{
    private static readonly FieldInfo? SentryInitField =
        AccessTools.Field(typeof(SentryService), "_sentryInit");

    [HarmonyPrefix]
    private static void SkipNativeGdExtensionShutdownPrefix()
    {
        if (SentryService.IsEnabled && SentryInitField?.GetValue(null) != null)
        {
            SentryInitField.SetValue(null, null);
            MainFile.Logger.Info("Skipped native Sentry GDExtension shutdown to avoid STS2 v0.107.1 startup hang.");
        }
    }
}
