using Imgeneus.World.Game.Inventory;
using System.Collections.Concurrent;

namespace Imgeneus.World.Game.Trade
{
    public class TradeRequest
    {
        public bool IsDecided_1;
        public bool IsDecided_2;

        public bool IsConfirmed_1;
        public bool IsConfirmed_2;

        /// <summary>
        /// Items, that are currently in trade window.
        /// </summary>
        public ConcurrentDictionary<(int CharacterId, byte Slot), Item> TradeItems { get; init; } = new ConcurrentDictionary<(int CharacterId, byte Slot), Item>();

        /// <summary>
        /// Money in trade window.
        /// </summary>
        public ConcurrentDictionary<int, uint> TradeMoney { get; init; } = new ConcurrentDictionary<int, uint>();
    }
}
