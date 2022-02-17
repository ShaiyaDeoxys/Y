using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Bank
{
    public interface IBankManager
    {
        void Init(int ownerId, IEnumerable<DbBankItem> items);

        /// <summary>
        /// Collection of bank items.
        /// </summary>
        ConcurrentDictionary<byte, BankItem> BankItems { get; }

        /// <summary>
        /// Adds an item to a player's bank
        /// </summary>
        Task<BankItem> AddBankItem(byte type, byte typeId, byte count);

        /// <summary>
        /// Attempts to take an item from the bank and put it into the player's inventory.
        /// </summary>
        /// <param name="slot">Bank slot where the item is.</param>
        Task<Item> TryClaimBankItem(byte slot);
    }
}
