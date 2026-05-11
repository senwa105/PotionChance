using System.Reflection;
using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace PotionChance.PotionChanceCode;

//You're recommended but not required to keep all your code in this package and all your assets in the PotionChance folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "PotionChance"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();
        string? dirName = Path.GetDirectoryName(assembly.Location);
        if (dirName is null)
        {
            throw new DirectoryNotFoundException("Could not determine the directory containing the executing assembly.");
        }
        Assembly.LoadFrom(dirName!.PathJoin("PotionChanceEstimators.dll"));
        
        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
        
        ModConfigRegistry.Register(ModId, new PotionChanceConfig());
        
        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}