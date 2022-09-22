using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Bank
{
    public class BankManager : IBankManager
    {
        private readonly ILogger<BankManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly IInventoryManager _inventoryManager;

        private int _ownerId;

        public BankManager(ILogger<BankManager> logger, IDatabase database, IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _database = database;
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _inventoryManager = inventoryManager;
#if DEBUG
            _logger.LogDebug("BankManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~BankManager()
        {
            _logger.LogDebug("BankManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId, IEnumerable<DbBankItem> items)
        {
            _ownerId = ownerId;

            foreach (var bankItem in items.Select(bi => new BankItem(bi)))
                BankItems.TryAdd(bankItem.Slot, bankItem);
        }

        public async Task Clear()
        {
            var oldItems = _database.BankItems.Where(b => b.UserId == _ownerId);
            _database.BankItems.RemoveRange(oldItems);

            foreach (var bankItem in BankItems)
            {
                var dbItem = new DbBankItem();
                dbItem.UserId = _ownerId;
                dbItem.Type = bankItem.Value.Type;
                dbItem.TypeId = bankItem.Value.TypeId;
                dbItem.Count = bankItem.Value.Count;
                dbItem.Slot = bankItem.Value.Slot;
                dbItem.ObtainmentTime = bankItem.Value.ObtainmentTime;
                dbItem.ClaimTime = bankItem.Value.ClaimTime;
                dbItem.IsClaimed = bankItem.Value.ClaimTime.HasValue;
                dbItem.IsDeleted = false;

                _database.BankItems.Add(dbItem);
            }

            await _database.SaveChangesAsync();
        }

        #endregion

        #region Items

        public ConcurrentDictionary<byte, BankItem> BankItems { get; init; } = new ConcurrentDictionary<byte, BankItem>();

        public BankItem AddBankItem(byte type, byte typeId, byte count)
        {
            var freeSlot = FindFreeBankSlot();

            // No available slots
            if (freeSlot == -1)
            {
                return null;
            }

            var bankItem = new BankItem((byte)freeSlot, type, typeId, count, DateTime.UtcNow);

            BankItems.TryAdd(bankItem.Slot, bankItem);

            return bankItem;
        }

        public Item TryClaimBankItem(byte slot)
        {
            if (!BankItems.TryRemove(slot, out var bankItem))
                return null;

            bankItem.ClaimTime = DateTime.UtcNow;

            var item = _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, bankItem), "from_bank");
            if (item == null)
            {
                BankItems.TryAdd(bankItem.Slot, bankItem);
                return null;
            }

            return item;
        }

        #endregion

        #region Helpers

        private int FindFreeBankSlot()
        {
            var maxSlot = 239;
            int freeSlot = -1;

            if (BankItems.Count > 0)
            {
                // Try to find any free slot.
                for (byte i = 0; i <= maxSlot; i++)
                {
                    if (!BankItems.TryGetValue(i, out _))
                    {
                        freeSlot = i;
                        break;
                    }
                }
            }
            else
            {
                freeSlot = 0;
            }

            return freeSlot;
        }

        #endregion
    }
}
