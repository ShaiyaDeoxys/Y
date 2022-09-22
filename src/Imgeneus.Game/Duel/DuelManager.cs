using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Duel
{
    /// <summary>
    /// Duel manager takes care of all duel requests.
    /// </summary>
    public class DuelManager : IDuelManager
    {
        private readonly ILogger<DuelManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly ITradeManager _tradeManager;
        private readonly IMovementManager _movementManager;
        private readonly IHealthManager _healthManager;
        private readonly IKillsManager _killsManager;
        private readonly IMapProvider _mapProvider;
        private readonly IInventoryManager _inventoryManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly Timer _duelRequestTimer = new Timer();
        private readonly Timer _duelStartTimer = new Timer();

        private uint _ownerId;

        public DuelManager(ILogger<DuelManager> logger, IGameWorld gameWorld, ITradeManager tradeManager, IMovementManager movementManager, IHealthManager healthManager, IKillsManager killsManager, IMapProvider mapProvider, IInventoryManager inventoryManager, ITeleportationManager teleportationManager)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _tradeManager = tradeManager;
            _movementManager = movementManager;
            _healthManager = healthManager;
            _killsManager = killsManager;
            _mapProvider = mapProvider;
            _inventoryManager = inventoryManager;
            _teleportationManager = teleportationManager;
            _duelRequestTimer.Interval = 10000; // 10 seconds.
            _duelRequestTimer.Elapsed += DuelRequestTimer_Elapsed;
            _duelRequestTimer.AutoReset = false;

            _duelStartTimer.Interval = 5000; // 5 seconds.
            _duelStartTimer.Elapsed += DuelStartTimer_Elapsed;
            _duelStartTimer.AutoReset = false;
#if DEBUG
            _logger.LogDebug("DuelManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~DuelManager()
        {
            _logger.LogDebug("DuelManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public Task Clear()
        {
            Cancel(_ownerId, DuelCancelReason.OpponentDisconnected);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _duelRequestTimer.Elapsed -= DuelRequestTimer_Elapsed;
            _duelStartTimer.Elapsed -= DuelStartTimer_Elapsed;
        }

        #endregion

        #region Request duel

        private uint _opponentId;
        public uint OpponentId
        {
            get => _opponentId;
            set
            {
                _opponentId = value;

                if (_opponentId == 0)
                    _duelRequestTimer.Stop();
                else
                    _duelRequestTimer.Start();

            }
        }

        private void DuelRequestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProcessResponse(OpponentId, DuelResponse.NoResponse);
        }

        public event Action<uint, DuelResponse> OnDuelResponse;

        public void ProcessResponse(uint senderId, DuelResponse response)
        {
            _duelRequestTimer.Stop();

            if (senderId != _ownerId)
                OnDuelResponse?.Invoke(senderId, response);

            if (response == DuelResponse.Approved)
            {
                _tradeManager.Start(_gameWorld.Players[_ownerId], _gameWorld.Players[OpponentId]);
            }
            else
            {
                Stop();
            }
        }

        #endregion

        #region Start

        public bool IsApproved { get; set; }

        private float _x;
        private float _z;

        public void Ready(float x, float z)
        {
            _x = x;
            _z = z;

            _duelStartTimer.Start();
        }

        private void DuelStartTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Start();
        }

        public bool IsStarted { get; private set; }

        public event Action OnStart;
        public void Start()
        {
            _movementManager.OnMove += MovementManager_OnMove;
            _healthManager.OnGotDamage += HealthManager_OnGotDamage;
            _healthManager.OnDead += HealthManager_OnDead;
            _teleportationManager.OnTeleporting += TeleportationManager_OnTeleporting;

            IsStarted = true;

            OnStart?.Invoke();
        }

        #endregion

        #region Cancel

        public event Action<uint, DuelCancelReason> OnCanceled;

        public void Cancel(uint senderId, DuelCancelReason reason)
        {
            if (OpponentId == 0)
                return;

            if (reason == DuelCancelReason.AdmitDefeat)
            {
                if (senderId == _ownerId)
                    _killsManager.Defeats++;
                else
                    _killsManager.Victories++;
            }

            if (senderId == _ownerId)
            {
                _gameWorld.Players.TryGetValue(OpponentId, out var opponent);
                if (opponent is not null)
                    opponent.DuelManager.Cancel(senderId, reason);
            }

            Stop();

            OnCanceled?.Invoke(senderId, reason);
        }

        private void Stop()
        {
            _movementManager.OnMove -= MovementManager_OnMove;
            _healthManager.OnGotDamage -= HealthManager_OnGotDamage;
            _healthManager.OnDead -= HealthManager_OnDead;
            _teleportationManager.OnTeleporting -= TeleportationManager_OnTeleporting;

            _tradeManager.Cancel();

            _x = 0;
            _z = 0;

            OpponentId = 0;
            IsApproved = false;
            IsStarted = false;
        }

        #endregion

        #region Lose & Win

        public event Action<bool> OnFinish;

        public void Lose()
        {
            _killsManager.Defeats++;
            OnFinish?.Invoke(false);

            _gameWorld.Players.TryGetValue(OpponentId, out var opponent);

            if (_tradeManager.Request != null)
            {
                foreach (var itemPair in _tradeManager.Request.TradeItems)
                {
                    if (itemPair.Key.CharacterId == _ownerId)
                    {
                        var item = _inventoryManager.RemoveItem(itemPair.Value, $"lost_duel_to_{OpponentId}");
                        _mapProvider.Map.AddItem(new MapItem(item, opponent, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ));
                    }
                }

                _tradeManager.Request.TradeMoney.TryGetValue(_ownerId, out var gold);
                if (gold > 0)
                {
                    var money = new Item((int)gold);
                    _mapProvider.Map.AddItem(new MapItem(money, opponent, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ));

                    var looser = _gameWorld.Players[_ownerId];
                    looser.InventoryManager.Gold -= gold;
                    looser.SendGoldUpdate();
                }
            }

            opponent.DuelManager.Win();

            Stop();
        }

        public void Win()
        {
            _killsManager.Victories++;
            OnFinish?.Invoke(true);

            Stop();
        }

        #endregion

        #region Trade

        public bool TryAddItem(byte bag, byte slot, byte quantity, byte slotInTradeWindow, out Item item)
        {
            return _tradeManager.TryAddItem(bag, slot, quantity, slotInTradeWindow, out item);
        }

        public bool TryRemoveItem(byte slotInTradeWindow)
        {
            return _tradeManager.TryRemoveItem(slotInTradeWindow);
        }

        public bool TryAddMoney(uint money, out uint resultMoney)
        {
            return _tradeManager.TryAddMoney(money, out resultMoney);
        }

        #endregion

        #region Helpers

        private void MovementManager_OnMove(uint senderId, float x, float y, float z, ushort angle, MoveMotion motion)
        {
            if (MathExtensions.Distance(x, _x, z, _z) >= 45)
                Cancel(_ownerId, DuelCancelReason.TooFarAway);
        }

        private void HealthManager_OnGotDamage(uint senderId, IKiller damageMaker, int damage)
        {
            if (damageMaker is Mob || damageMaker.Id != OpponentId)
                Cancel(_ownerId, DuelCancelReason.MobAttack);
        }

        private void HealthManager_OnDead(uint senderId, IKiller killer)
        {
            Lose();
        }

        private void TeleportationManager_OnTeleporting(uint senderId, ushort mapId, float x, float y, float z, bool teleportedByAdmin, bool summonedByAdmin)
        {
            if (_mapProvider.Map.Id != mapId|| MathExtensions.Distance(x, _x, z, _z) >= 45)
                Cancel(_ownerId, DuelCancelReason.TooFarAway);
        }

        #endregion
    }
}
