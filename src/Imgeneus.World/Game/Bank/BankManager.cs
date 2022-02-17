using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Inventory;
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
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IInventoryManager _inventoryManager;

        private int _ownerId;

        public BankManager(ILogger<BankManager> logger, IDatabase database, IDatabasePreloader databasePreloader, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _database = database;
            _databasePreloader = databasePreloader;
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

        #endregion

        #region Items

        public ConcurrentDictionary<byte, BankItem> BankItems { get; init; } = new ConcurrentDictionary<byte, BankItem>();

        public async Task<BankItem> AddBankItem(byte type, byte typeId, byte count)
        {
            var freeSlot = FindFreeBankSlot();

            // No available slots
            if (freeSlot == -1)
            {
                return null;
            }

            var bankItem = new BankItem((byte)freeSlot, type, typeId, count);

            BankItems.TryAdd(bankItem.Slot, bankItem);

            var dbItem = new DbBankItem();
            dbItem.UserId = _ownerId;
            dbItem.Type = bankItem.Type;
            dbItem.TypeId = bankItem.TypeId;
            dbItem.Count = count;
            dbItem.Slot = bankItem.Slot;
            dbItem.ObtainmentTime = DateTime.UtcNow;
            dbItem.ClaimTime = null;
            dbItem.IsClaimed = false;
            dbItem.IsDeleted = false;

            _database.BankItems.Add(dbItem);
            await _database.SaveChangesAsync();

            return bankItem;
        }

        public async Task<Item> TryClaimBankItem(byte slot)
        {
            if (!BankItems.TryGetValue(slot, out var bankItem))
                return null;

            var item = _inventoryManager.AddItem(new Item(_databasePreloader, bankItem));

            if (item == null)
                return null;

            BankItems.TryRemove(slot, out _);

            var dbItem = _database.BankItems.First(ubi => ubi.UserId == _ownerId && ubi.Slot == slot && !ubi.IsClaimed);
            dbItem.ClaimTime = DateTime.UtcNow;
            dbItem.IsClaimed = true;

            var result = await _database.SaveChangesAsync();
            return result > 0 ? item : null;
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
