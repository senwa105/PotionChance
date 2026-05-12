using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using PotionChanceEstimators;

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
        _tracker.EstimateUpdated += OnEstimateUpdated;
        ConnectSignals();

        OnEstimateUpdated(_tracker.TotalEstimatedChance, _tracker.HasEliteBonus || _tracker.HasWhiteBeastStatue);
    }

    public override void _ExitTree()
    {
        _tracker.EstimateUpdated -= OnEstimateUpdated;
    }

    private List<IHoverTip> CreateHoverTips()
    {
        List<IHoverTip> hoverTips = new List<IHoverTip>();
        
        // Estimated chance
        LocString estimatedChanceDesc = new LocString(
            LocTable, 
            "POTION_CHANCE-ESTIMATED_POTION_CHANCE.description"
        );
        estimatedChanceDesc.Add("EstimatedChance", $"{_tracker.EstimatedChance:P1}");
        
        HoverTip estimatedChanceHoverTip = new HoverTip(
            new LocString(LocTable, "POTION_CHANCE-ESTIMATED_POTION_CHANCE.title"), 
            estimatedChanceDesc
        );
        
        hoverTips.Add(estimatedChanceHoverTip);

        // True chance
        if (PotionChanceConfig.ShowTrueChance)
        {
            LocString trueChanceDesc = new LocString(
                LocTable, 
                "POTION_CHANCE-TRUE_POTION_CHANCE.description"
            );
            
            if (_tracker.TrueChance is { } chance)
                trueChanceDesc.Add("TrueChance", $"{chance:P1}");
            else
                trueChanceDesc.Add("TrueChance", "N/A");

            HoverTip trueChanceHoverTip = new HoverTip(
                new LocString(LocTable, "POTION_CHANCE-TRUE_POTION_CHANCE.title"),
                trueChanceDesc
            );
            
            hoverTips.Add(trueChanceHoverTip);
        }
        
        // Elite bonus
        if (_tracker.HasEliteBonus)
            hoverTips.Add(EliteBonusHoverTip);
        
        // White Beast Statue bonus
        if (_tracker.HasWhiteBeastStatue)
            hoverTips.Add(WbsBonusHoverTip);
        
        // Belief distribution
        Belief belief = _tracker.Belief;
        LocString distributionDesc = new LocString(
            LocTable, 
            "POTION_CHANCE-POTION_CHANCE_DISTRIBUTION.description"
        );
        
        for (int i = 0; i <= 10; i++)
        {
            distributionDesc.Add($"P{i}", $"{belief[i]:F3}");
        }

        AddWeightColors(distributionDesc);

        HoverTip distributionHoverTip = new HoverTip(
            new LocString(LocTable, "POTION_CHANCE-POTION_CHANCE_DISTRIBUTION.title"),
            distributionDesc
        );
        
        hoverTips.Add(distributionHoverTip);

        return hoverTips;
    }

    private void AddWeightColors(LocString distributionDesc)
    {
        Belief belief = _tracker.Belief;
        
        for (int i = 0; i <= 10; i++)
        {
            Color gradated = StsColors.red.Lerp(StsColors.green, belief[i]);
            Color color = belief[i] > 0f ? gradated : StsColors.gray;
            distributionDesc.Add($"Color{i}", $"{color.ToHtml()}");
        }
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

    private void OnEstimateUpdated(float newValue, bool hasBonus)
    {
        _potionChanceLabel.Text = $"{newValue:P1}";

        Color color = hasBonus ? StsColors.green : StsColors.cream;
        _potionChanceLabel.AddThemeColorOverride(ThemeConstants.Label.FontColor, color);
    }
}