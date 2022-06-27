using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Trade
{
    public interface ITradeManager : ISessionedService
    {
        void Init(uint ownerId);

        /// <summary>
        /// With whom player is currently trading.
        /// </summary>
        uint PartnerId { get; set; }

        /// <summary>
        /// Represents currently open trade window.
        /// </summary>
        TradeRequest Request { get; set; }

        /// <summary>
        /// Starts trade between 2 players.
        /// </summary>
        void Start(Character player1, Character player2);

        /// <summary>
        /// Cancels trade.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Adds item to trade window.
        /// </summary>
        /// <param name="bag">item bag in inventory</param>
        /// <param name="slot">item slot in inventory</param>
        /// <param name="quantity">item count</param>
        /// <param name="slotInWindow">slot in trade window</param>
        /// <returns>true if item was added</returns>
        bool TryAddItem(byte bag, byte slot, byte quantity, byte slotInWindow, out Item tradeItem);

        /// <summary>
        /// Removes item from trade window.
        /// </summary>
        /// <param name="slotInWindow">slot in trade window</param>
        /// <returns>true if item was removed</returns>
        bool TryRemoveItem(byte slotInWindow);

        /// <summary>
        /// Tries to add money to trade.
        /// </summary>
        bool TryAddMoney(uint money, out uint resultMoney);

        /// <summary>
        /// Called, when user clicks "Decide" button.
        /// </summary>
        void TraderDecideConfirm();

        /// <summary>
        /// Called, when user clicks "Decide" button again, which declines previous decide.
        /// </summary>
        void TradeDecideDecline();

        /// <summary>
        /// Called, when user clicks "Confirm" button.
        /// </summary>
        void Confirmed();

        /// <summary>
        /// Called, when user clicks "Confirm" button again, which declines previous confirm.
        /// </summary>
        void ConfirmDeclined();

        /// <summary>
        /// Finishes trade and assignes items to right players.
        /// </summary>
        void FinishSuccessful();
    }
}
