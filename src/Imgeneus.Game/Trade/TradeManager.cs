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

        private uint _ownerId;

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

        public void Init(uint ownerId)
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

        public uint PartnerId { get; set; }

        public TradeRequest Request { get; set; }

        public void Start(Character player1, Character player2)
        {
            var request = new TradeRequest();
            player1.TradeManager.Request = request;
            player2.TradeManager.Request = request;
        }

        #endregion

        #region Trade finish

        public void Cancel()
        {
            ClearTrade();
        }

        private void ClearTrade()
        {
            PartnerId = 0;

            if (Request is null)
                return;

            foreach (var key in Request.TradeItems.Keys.Where(x => x.CharacterId == _ownerId).ToList())
                Request.TradeItems.TryRemove(key, out var itm);

            Request.TradeMoney.TryRemove(_ownerId, out var m);

            if (Request.TradeItems.IsEmpty && Request.TradeMoney.IsEmpty)
                Request = null;
        }

        public void FinishSuccessful()
        {
            foreach (var item in Request.TradeItems.Where(x => x.Key.CharacterId == _ownerId))
            {
                var tradeItem = item.Value;
                var resultItm = _inventoryManager.RemoveItem(tradeItem, $"give_in_trade_to_{PartnerId}");

                if (_gameWorld.Players[PartnerId].InventoryManager.AddItem(resultItm, $"trade_from_{_ownerId}") is null) // No place for this item.
                {
                    _inventoryManager.AddItem(resultItm, "trade_no_place");
                }
            }

            if (Request.TradeMoney.ContainsKey(_ownerId) && Request.TradeMoney[_ownerId] > 0)
            {
                _inventoryManager.Gold = _inventoryManager.Gold - Request.TradeMoney[_ownerId];
                _gameWorld.Players[PartnerId].InventoryManager.Gold = _gameWorld.Players[PartnerId].InventoryManager.Gold + Request.TradeMoney[_ownerId];
            }

            ClearTrade();
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

            if (Request.TradeItems.Any(x => x.Value == item))
            {
                _logger.LogWarning("Player {id} tries add item to trade twice", _ownerId);
                return false;
            }

            item.TradeQuantity = item.Count > quantity ? quantity : item.Count;
            Request.TradeItems.TryAdd((_ownerId, slotInWindow), item);
            return true;
        }

        public bool TryRemoveItem(byte slotInWindow)
        {
            Request.TradeItems.TryRemove((_ownerId, slotInWindow), out var removed);
            if (removed is null)
            {
                _logger.LogWarning("Player {id} has no item at this slot", _ownerId);
                return false;
            }

            TradeDecideDecline();
            return true;
        }

        public bool TryAddMoney(uint money, out uint resultMoney)
        {
            if (money < _inventoryManager.Gold)
            {
                Request.TradeMoney[_ownerId] = money;
            }
            else
            {
                _logger.LogWarning("Player {id} tries to add more money that he has in inventory", _ownerId);
                Request.TradeMoney[_ownerId] = _inventoryManager.Gold;
            }

            resultMoney = Request.TradeMoney[_ownerId];
            return true;
        }

        #endregion

        #region Decide & Confirm

        public void TraderDecideConfirm()
        {
            if (Request.IsDecided_1)
                Request.IsDecided_2 = true;
            else
                Request.IsDecided_1 = true;
        }

        public void TradeDecideDecline()
        {
            Request.IsDecided_1 = false;
            Request.IsDecided_2 = false;
        }


        public void Confirmed()
        {
            if (Request.IsConfirmed_1)
                Request.IsConfirmed_2 = true;
            else
                Request.IsConfirmed_1 = true;
        }

        public void ConfirmDeclined()
        {
            Request.IsConfirmed_1 = false;
            Request.IsConfirmed_2 = false;
        }

        #endregion
    }
}
