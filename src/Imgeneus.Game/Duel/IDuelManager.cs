using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Duel
{
    public interface IDuelManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Duel opponent.
        /// </summary>
        uint OpponentId { get; set; }

        /// <summary>
        /// On some response got from player.
        /// </summary>
        event Action<uint, DuelResponse> OnDuelResponse;

        /// <summary>
        /// Starts or stops duel.
        /// </summary>
        void ProcessResponse(uint senderId, DuelResponse response);

        /// <summary>
        /// Adds item from inventory to duel trade.
        /// </summary>
        /// <param name="bag">inventory tab</param>
        /// <param name="slot">inventory slot</param>
        /// <param name="quantity">number of items</param>
        /// <param name="slotInTradeWindow">slot in trade window</param>
        /// <param name="item">item in trade window</param>
        /// <returns>true if item was added</returns>
        bool TryAddItem(byte bag, byte slot, byte quantity, byte slotInTradeWindow, out Item item);

        /// <summary>
        /// Removes item from trade window.
        /// </summary>
        /// <param name="slotInTradeWindow"></param>
        /// <returns>true is item was removed</returns>
        bool TryRemoveItem(byte slotInTradeWindow);

        /// <summary>
        /// Adds money to trade.
        /// </summary>
        bool TryAddMoney(uint money, out uint resultMoney);

        /// <summary>
        /// Is duel approved?
        /// </summary>
        bool IsApproved { get; set; }

        /// <summary>
        /// Ready 5 sec timer starts.
        /// </summary>
        void Ready(float x, float z);

        /// <summary>
        /// Event, that is fired as soon as duel starts.
        /// </summary>
        event Action OnStart;

        /// <summary>
        /// Is duel in progress?
        /// </summary>
        bool IsStarted { get; }

        /// <summary>
        /// Starts duel.
        /// </summary>
        void Start();

        /// <summary>
        /// Event, that is fired as soon as duel is canceled.
        /// </summary>
        event Action<uint, DuelCancelReason> OnCanceled;

        /// <summary>
        /// Cancels duel.
        /// </summary>
        void Cancel(uint senderId, DuelCancelReason reason);

        /// <summary>
        /// Event, that is fired on lose or win.
        /// </summary>
        event Action<bool> OnFinish;

        /// <summary>
        /// Loses duel.
        /// </summary>
        void Lose();

        /// <summary>
        /// Wins duel.
        /// </summary>
        void Win();
    }
}
