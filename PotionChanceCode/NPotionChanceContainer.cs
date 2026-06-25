using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace PotionChance.PotionChanceCode;

public partial class NPotionChanceContainer : NClickableControl
{
    public const string ScenePath = "res://PotionChance/scenes/potion_chance_container.tscn";
    
    private const string LocTable = "static_hover_tips";
    
    private static readonly HoverTip EliteBonusHoverTip = new HoverTip(
        new LocString(LocTable, "POTION_CHANCE-ELITE_BONUS.title"),
        new LocString(LocTable, "POTION_CHANCE-ELITE_BONUS.description")
    );

    private static readonly HoverTip WbsBonusHoverTip = new HoverTip(
        new LocString(LocTable, "POTION_CHANCE-WHITE_BEAST_STATUE_BONUS.title"), 
        new LocString(LocTable, "POTION_CHANCE-WHITE_BEAST_STATUE_BONUS.description")
    );

    private PotionChanceTracker _tracker = null!;
    
    private Label _potionChanceLabel = null!;
    
    public override void _Ready()
    {
        _potionChanceLabel = GetNode<Label>("PotionChanceLabel");

        _tracker = ModelDb.Singleton<PotionChanceTracker>();
        _tracker.ChanceUpdated += OnChanceUpdated;
        ConnectSignals();

        OnChanceUpdated(_tracker.TotalChance, _tracker.HasEliteBonus || _tracker.HasWhiteBeastStatue);
    }

    public override void _ExitTree()
    {
        _tracker.ChanceUpdated -= OnChanceUpdated;
    }

    public void UpdateNavigation(List<NPotionHolder> holders)
    {
        Control? control = NRun.Instance?.GlobalUi.RelicInventory.RelicNodes.FirstOrDefault<NRelicInventoryHolder>();
        if (control == null)
        {
            MainFile.Logger.Error("Failed to find NRelicInventoryHolder");
            return;
        }
        
        FocusNeighborLeft = NRun.Instance?.GlobalUi.TopBar.Gold.GetPath() ?? new NodePath();
        FocusNeighborRight = holders[0].GetPath();
        FocusNeighborBottom = control.GetPath();
        FocusNeighborTop = GetPath();
        
        holders[0].FocusNeighborLeft = GetPath();
    }

    private List<IHoverTip> CreateHoverTips()
    {
        List<IHoverTip> hoverTips = new List<IHoverTip>();

        // Potion chance
        LocString chanceDesc = new LocString(
            LocTable, 
            "POTION_CHANCE-CHANCE.description"
        );
        
        chanceDesc.Add("Chance", $"{_tracker.Chance:P1}");

        HoverTip chanceHoverTip = new HoverTip(
            new LocString(LocTable, "POTION_CHANCE-CHANCE.title"),
            chanceDesc
        );

        hoverTips.Add(chanceHoverTip);
        
        // Elite bonus
        if (_tracker.HasEliteBonus)
            hoverTips.Add(EliteBonusHoverTip);
        
        // White Beast Statue bonus
        if (_tracker.HasWhiteBeastStatue)
            hoverTips.Add(WbsBonusHoverTip);

        return hoverTips;
    }
    
    protected override void OnFocus()
    {
        List<IHoverTip> hoverTips = CreateHoverTips();
        NHoverTipSet.CreateAndShow(this, hoverTips)?.SetGlobalPosition(base.GlobalPosition + new Vector2(0f, base.Size.Y + 20f));
    }
    
    protected override void OnUnfocus()
    {
        NHoverTipSet.Remove(this);
    }

    private void OnChanceUpdated(float newValue, bool hasBonus)
    {
        _potionChanceLabel.Text = $"{newValue:P1}";

        Color color = hasBonus ? StsColors.green : StsColors.cream;
        _potionChanceLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, color);
    }
}