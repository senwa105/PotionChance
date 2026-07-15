using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Rooms;

namespace PotionChance.PotionChanceCode;

public class PotionChanceTracker : IDisposable
{
    private static bool _initialized = false;
    
    public static PotionChanceTracker? Instance { get; private set; }
    
    private Player? _player;
    
    private Player Player => _player ?? throw new InvalidOperationException("Player accessed before initialization.");

    private RunState _runState;
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
    
    public static void Initialize()
    {
        if (_initialized) return;
        
        RunManager.Instance.RunStarted += OnRunStarted;
        
        _initialized = true;
        MainFile.Logger.Info("PotionChanceTracker initialized.");
    }

    private static void OnRunStarted(RunState runState)
    {
        if (Instance != null)
        {
            MainFile.Logger.Info("Disposing previous PotionChanceTracker instance on run resume.");
            Instance.Dispose(); 
        }
        
        Instance = new PotionChanceTracker(runState);
        MainFile.Logger.Info("New PotionChanceTracker constructed.");
    }

    private PotionChanceTracker(RunState runState)
    {
        RunManager.Instance.RoomEntered += OnRoomEntered;
        
        PotionOddsEvents.PotionRolled += OnPotionRolled;
        PotionOddsEvents.OddsOverridden += OnOddsOverridden;

        _runState = runState;
        InitPlayer();
    }

    private void InitPlayer()
    {
        Player? me = LocalContext.GetMe(_runState.Players);
        if (me == null)
        { 
            MainFile.Logger.Warn("PotionChanceTracker failed to get player.");
            return;
        }

        _player = me;
        Player.RelicObtained += OnRelicObtained;
        Player.RelicRemoved += OnRelicRemoved;
        
        Chance = Player.PlayerOdds.PotionReward.CurrentValue;
        HasEliteBonus = _runState.CurrentRoom?.RoomType == RoomType.Elite;
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

    private void OnRoomEntered()
    {
        HasEliteBonus = _runState.CurrentRoom!.RoomType == RoomType.Elite;
        ChanceUpdated?.Invoke(TotalChance, HasEliteBonus || HasWhiteBeastStatue);
    }
    
    public void Dispose()
    {
        PotionOddsEvents.PotionRolled -= OnPotionRolled;
        PotionOddsEvents.OddsOverridden -= OnOddsOverridden;
        RunManager.Instance.RoomEntered -= OnRoomEntered;
        
        if (_player != null)
        {
            _player.RelicObtained -= OnRelicObtained;
            _player.RelicRemoved -= OnRelicRemoved;
        }
    }
}