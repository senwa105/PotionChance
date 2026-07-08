using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;

namespace PotionChance.PotionChanceCode;

public class PotionChanceTracker : SingletonModel
{
    private Player? _player;
    
    private Player Player => _player ?? throw new InvalidOperationException("Player accessed before initialization.");
    
    public event Action<float, bool>? ChanceUpdated;

    public bool HasEliteBonus { get; private set; }

    public bool HasWhiteBeastStatue { get; private set; }

    public float Chance { get; private set; }
    
    public float TotalChance
    {
        get
        {
            float eliteBonus = HasEliteBonus ? 0.125f : 0;
            float wbsBonus = HasWhiteBeastStatue ? 1.0f : 0;
            return Math.Min(1f, Chance + eliteBonus + wbsBonus);
        }
    }

    public override bool ShouldReceiveCombatHooks => true;

    public PotionChanceTracker()
    {
        ModHelper.SubscribeForRunStateHooks(Id.Entry, RunSubModels);
        
        RunManager.Instance.RunStarted += OnRunStarted;
        PotionOddsEvents.PotionRolled += OnPotionRolled;
        PotionOddsEvents.OddsOverridden += OnOddsOverridden;
    }
    
    private IEnumerable<AbstractModel> RunSubModels(RunState runState)
    {
        return [this];
    }
    
    private void OnRunStarted(RunState runState)
    {
        Player? me = LocalContext.GetMe(runState.Players);
        if (me == null)
        { 
            MainFile.Logger.Warn("PotionChanceTracker failed to get player.");
            return;
        }

        _player = me;
        Player.RelicObtained += OnRelicObtained;
        Player.RelicRemoved += OnRelicRemoved;
        
        Chance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"Chance initialized to {Chance}.");
        
        HasEliteBonus = runState.CurrentRoom?.RoomType == RoomType.Elite;
        HasWhiteBeastStatue = Player.Relics.Any(r => r is WhiteBeastStatue);
        
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnPotionRolled(RoomType roomType)
    {
        HasEliteBonus = roomType == RoomType.Elite;
        HasWhiteBeastStatue = Player.Relics.Any(r => r is WhiteBeastStatue);

        Chance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"Chance updated to {Chance}.");
        
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnOddsOverridden()
    {
        Chance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"Chance overridden to {Chance}.");
        
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnRelicObtained(RelicModel relic)
    {
        if (relic is WhiteBeastStatue)
            HasWhiteBeastStatue = true;
        
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnRelicRemoved(RelicModel relic)
    {
        if (relic is WhiteBeastStatue)
            HasWhiteBeastStatue = false;
        
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        HasEliteBonus = room.RoomType == RoomType.Elite;
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
        
        return Task.CompletedTask;
    }
}