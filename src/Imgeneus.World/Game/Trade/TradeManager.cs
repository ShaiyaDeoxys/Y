using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Trade
{
    /// <summary>
    /// Trade manager takes care of all trade requests.
    /// </summary>
    public class TradeManager : ITradeManager
    {
        private readonly ILogger<TradeManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly IInventoryManager _inventoryManager;

        private int _ownerId;

        public TradeManager(ILogger<TradeManager> logger, IGameWorld gameWorld, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _inventoryManager = inventoryManager;
#if DEBUG
            _logger.LogDebug("TradeManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~TradeManager()
        {
            _logger.LogDebug("TradeManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        public Task Clear()
        {
            Cancel();
            return Task.CompletedTask;
        }

        #endregion

        #region Trade start

        public int TradePartnerId { get; set; }

        public TradeRequest TradeRequest { get; set; }

        public void Start(Character player1, Character player2)
        {
            var request = new TradeRequest();
            player1.TradeManager.TradeRequest = request;
            player2.TradeManager.TradeRequest = request;
        }

        #endregion

        #region Trade finish

        public event Action OnCanceled;

        public void Cancel()
        {
            ClearTrade(out var partner);
            if (partner != null)
                partner.TradeManager.Cancel();

            OnCanceled?.Invoke();
        }

        private void ClearTrade(out Character parther)
        {
            if (_gameWorld.Players.ContainsKey(TradePartnerId))
                parther = _gameWorld.Players[TradePartnerId];
            else
                parther = null;

            TradePartnerId = 0;
            TradeRequest.TradeItems.Clear();
            TradeRequest.TradeMoney.Clear();

            TradeRequest = null;
        }

        public void FinishSuccessful(bool clearTradeSession = false)
        {
            foreach (var item in TradeRequest.TradeItems.Where(x => x.Key.CharacterId == _ownerId))
            {
                var tradeItem = item.Value;
                var resultItm = _inventoryManager.RemoveItem(tradeItem);

                if (_gameWorld.Players[TradePartnerId].InventoryManager.AddItem(resultItm) is null) // No place for this item.
                {
                    _inventoryManager.AddItem(resultItm);
                }
            }

            if (TradeRequest.TradeMoney.ContainsKey(_ownerId) && TradeRequest.TradeMoney[_ownerId] > 0)
            {
                _inventoryManager.Gold = _inventoryManager.Gold - TradeRequest.TradeMoney[_ownerId];
                _gameWorld.Players[TradePartnerId].InventoryManager.Gold = _gameWorld.Players[TradePartnerId].InventoryManager.Gold + TradeRequest.TradeMoney[_ownerId];
            }

            if (clearTradeSession)
                ClearTrade(out var p);
            else
                _gameWorld.Players[TradePartnerId].TradeManager.FinishSuccessful(true);
        }

        #endregion

        #region Add & Remove item

        public bool TryAddItem(byte bag, byte slot, byte quantity, byte slotInWindow, out Item tradeItem)
        {
            _inventoryManager.InventoryItems.TryGetValue((bag, slot), out var item);
            tradeItem = item;
            if (item is null)
            {
                _logger.LogWarning("Player {id} does not contain such item in inventory", _ownerId);
                return false;
            }

            if (TradeRequest.TradeItems.Any(x => x.Value == item))
            {
                _logger.LogWarning("Player {id} tries add item to trade twice", _ownerId);
                return false;
            }

            item.TradeQuantity = item.Count > quantity ? quantity : item.Count;
            TradeRequest.TradeItems.TryAdd((_ownerId, slotInWindow), item);
            return true;
        }

        public bool TryRemoveItem(byte slotInWindow)
        {
            TradeRequest.TradeItems.TryRemove((_ownerId, slotInWindow), out var removed);
            if (removed is null)
            {
                _logger.LogWarning("Player {id} has no item at this slot", _ownerId);
                return false;
            }

            TradeDecideDecline();
            return true;
        }

        public bool TryAddMoney(uint money)
        {
            if (money < _inventoryManager.Gold)
            {
                TradeRequest.TradeMoney[_ownerId] = money;
            }
            else
            {
                _logger.LogWarning("Player {id} tries to add more money that he has in inventory", _ownerId);
                TradeRequest.TradeMoney[_ownerId] = _inventoryManager.Gold;
            }

            return true;
        }

        #endregion

        #region Decide & Confirm

        public void TraderDecideConfirm()
        {
            if (TradeRequest.IsDecided_1)
                TradeRequest.IsDecided_2 = true;
            else
                TradeRequest.IsDecided_1 = true;
        }

        public void TradeDecideDecline()
        {
            TradeRequest.IsDecided_1 = false;
            TradeRequest.IsDecided_2 = false;
        }


        public void Confirmed()
        {
            if (TradeRequest.IsConfirmed_1)
                TradeRequest.IsConfirmed_2 = true;
            else
                TradeRequest.IsConfirmed_1 = true;
        }

        public void ConfirmDeclined()
        {
            TradeRequest.IsConfirmed_1 = false;
            TradeRequest.IsConfirmed_2 = false;
        }

        #endregion
    }
}
