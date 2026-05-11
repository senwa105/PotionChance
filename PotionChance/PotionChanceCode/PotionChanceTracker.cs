using BaseLib.Abstracts;
using BaseLib.Config;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;
using PotionChanceEstimators;

namespace PotionChance.PotionChanceCode;

public class PotionChanceTracker : CustomSingletonModel
{
    private IEstimator _estimator;

    private Player? _player;
    
    private Player Player => _player ?? throw new InvalidOperationException("Player accessed before initialization.");

    private int _currentFloor;
    
    private RunSaveData _saveData;
    
    public event Action<float, bool>? EstimateUpdated;

    public bool HasEliteBonus { get; private set; }

    public bool HasWhiteBeastStatue { get; private set; }

    public float? TrueChance { get; private set; }

    public float EstimatedChance => _estimator.GetExpectedChance();

    public Belief Belief => _estimator.Belief;
    
    public float TotalEstimatedChance
    {
        get
        {
            float eliteBonus = HasEliteBonus ? 0.125f : 0;
            float wbsBonus = HasWhiteBeastStatue ? 1.0f : 0;
            return Math.Min(1f, EstimatedChance + eliteBonus + wbsBonus);
        }
    }

    public PotionChanceTracker() : base(false, true)
    {
        _estimator = new HmmEstimator(); // Dummy; will be overridden
        _saveData = new RunSaveData(); // Dummy; will be overridden
        
        RunManager.Instance.RunStarted += OnRunStarted;
        PotionOddsEvents.PotionRolled += OnPotionRolled;
        PotionOddsEvents.OddsOverridden += OnOddsOverridden;
        PotionOddsEvents.RunEnded += OnRunEnded;
    }

    private IEstimator ConstructEstimator(Belief belief)
    {
        return PotionChanceConfig.Estimator switch
        {
            PotionChanceConfig.EstimatorType.Simple => new SimpleEstimator(belief),
            _ => new HmmEstimator(belief)
        };
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
        
        TrueChance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"TrueChance initialized to {TrueChance}.");

        _currentFloor = runState.TotalFloor;
        MainFile.Logger.Info($"Current floor initialized to {_currentFloor}.");
        
        bool isMultiplayer = runState.Players.Count > 1;
        _saveData = isMultiplayer 
            ? PotionChanceConfig.RunSaveMultiplayer 
            : PotionChanceConfig.RunSaveSingleplayer;

        string seed = runState.Rng.StringSeed;
        bool isReloadedRun = seed == _saveData.Seed && 
                             _currentFloor > 0 && 
                             _currentFloor >= _saveData.LastSavedFloor;

        if (!isReloadedRun)
        {
            // New run
            _saveData.Clear();
            _saveData.Seed = seed;
            _saveData.LastSavedFloor = _currentFloor;
            ModConfig.SaveDebounced<PotionChanceConfig>();
        
            MainFile.Logger.Info($"New run started (Seed: {seed}, Floor: {_currentFloor}). Cleared belief histories.");
        } else MainFile.Logger.Info($"Run reloaded (Seed: {seed}, Floor: {_currentFloor})");
        
        Belief? savedBelief = _saveData.BeliefHistory[_saveData.LastSavedFloor];
        
        if (isReloadedRun && savedBelief is {} saved)
        {
            _estimator = ConstructEstimator(saved);
            MainFile.Logger.Info($"Belief retrieved from floor {_saveData.LastSavedFloor}. Initialized to \n\tfull belief: {Belief}\n\tpositive only: {Belief.ToStringPositiveOnly()}");
        }
        else
        {
            _estimator = ConstructEstimator(Belief.FromKnownChance(TrueChance ?? 0.4f));
            MainFile.Logger.Info($"Belief initialized to \n\tfull belief: {Belief}\n\tpositive only: {Belief.ToStringPositiveOnly()}");
        }
        
        MainFile.Logger.Info($"EstimatedChance initialized to {EstimatedChance}.");
        
        HasEliteBonus = runState.CurrentRoom?.RoomType == RoomType.Elite;
        HasWhiteBeastStatue = Player.Relics.Any(r => r is WhiteBeastStatue);
        
        EstimateUpdated?.Invoke(TotalEstimatedChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnPotionRolled(bool dropped, RoomType roomType)
    {
        HasEliteBonus = roomType == RoomType.Elite;
        HasWhiteBeastStatue = Player.Relics.Any(r => r is WhiteBeastStatue);
        
        TrueChance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"TrueChance updated to {TrueChance}.");
        
        _estimator.UpdateBelief(dropped, HasEliteBonus, HasWhiteBeastStatue);
        MainFile.Logger.Info($"Belief updated to \n\tfull belief: {Belief}\n\tpositive only: {Belief.ToStringPositiveOnly()}");
        MainFile.Logger.Info($"EstimatedChance updated to {EstimatedChance}.");
        
        EstimateUpdated?.Invoke(TotalEstimatedChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnOddsOverridden(float newValue)
    {
        TrueChance = Player.PlayerOdds.PotionReward.CurrentValue;
        MainFile.Logger.Info($"TrueChance overridden to {TrueChance}.");
    }

    private void OnRelicObtained(RelicModel relic)
    {
        if (relic is WhiteBeastStatue)
            HasWhiteBeastStatue = true;
        
        EstimateUpdated?.Invoke(TotalEstimatedChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnRelicRemoved(RelicModel relic)
    {
        if (relic is WhiteBeastStatue)
            HasWhiteBeastStatue = false;
        
        EstimateUpdated?.Invoke(TotalEstimatedChance, HasEliteBonus || HasWhiteBeastStatue);
    }

    private void OnRunEnded()
    {
        if (_player != null)
        {
            _player.RelicObtained -= OnRelicObtained;
            _player.RelicRemoved -= OnRelicRemoved;
            _player = null;
            MainFile.Logger.Info($"Successfully unsubscribed from RelicObtained and RelicRemoved.");
        }
        else MainFile.Logger.Warn("Cannot unsubscribe from RelicObtained and RelicRemoved because _player is null; likely leaked.");

        HasWhiteBeastStatue = false;
        HasEliteBonus = false;

        _saveData.Clear();
        ModConfig.SaveDebounced<PotionChanceConfig>();
    }

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        _saveData.BeliefHistory[_currentFloor] = Belief;
        _saveData.LastSavedFloor = _currentFloor;
        ModConfig.SaveDebounced<PotionChanceConfig>();
        MainFile.Logger.Info($"Saved belief on floor {_currentFloor} as \n\tfull belief: {Belief}\n\tpositive only: {Belief.ToStringPositiveOnly()}");
        
        _currentFloor += 1;

        HasEliteBonus = room.RoomType == RoomType.Elite;
        EstimateUpdated?.Invoke(TotalEstimatedChance, HasEliteBonus || HasWhiteBeastStatue);
        
        return Task.CompletedTask;
    }
}