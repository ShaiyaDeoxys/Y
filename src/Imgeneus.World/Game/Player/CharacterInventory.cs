using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Skills;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        #region Inventory

        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        public readonly ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();

        /// <summary>
        /// Adds item to player's inventory.
        /// </summary>
        /// <param name="itemType">item type</param>
        /// <param name="itemTypeId">item type id</param>
        /// <param name="count">how many items</param>
        public Item AddItemToInventory(Item item)
        {
            /*// Find free space.
            var free = FindFreeSlotInInventory();

            // Calculated bag slot can not be 0, because 0 means worn item. Newly created item can not be worn.
            if (free.Bag == 0 || free.Slot == -1)
            {
                return null;
            }

            item.Bag = free.Bag;
            item.Slot = (byte)free.Slot;

            _taskQueue.Enqueue(ActionType.SAVE_ITEM_IN_INVENTORY,
                               Id, item.Type, item.TypeId, item.Count, item.Quality, item.Bag, item.Slot,
                               item.Gem1 is null ? 0 : item.Gem1.TypeId,
                               item.Gem2 is null ? 0 : item.Gem2.TypeId,
                               item.Gem3 is null ? 0 : item.Gem3.TypeId,
                               item.Gem4 is null ? 0 : item.Gem4.TypeId,
                               item.Gem5 is null ? 0 : item.Gem5.TypeId,
                               item.Gem6 is null ? 0 : item.Gem6.TypeId,
                               item.DyeColor.IsEnabled, item.DyeColor.Alpha, item.DyeColor.Saturation, item.DyeColor.R, item.DyeColor.G, item.DyeColor.B, item.CreationTime, item.ExpirationTime);

            InventoryItems.TryAdd((item.Bag, item.Slot), item);

            if (item.ExpirationTime != null)
            {
                item.OnExpiration += CharacterItem_OnExpiration;
            }

            _logger.LogDebug($"Character {Id} got item {item.Type} {item.TypeId}");
            return item;*/
            return null;
        }

        /// <summary>
        /// Removes item from inventory
        /// </summary>
        /// <param name="item">item, that we want to remove</param>
        public Item RemoveItemFromInventory(Item item)
        {
            /*  // If we are giving consumable item.
              if (item.TradeQuantity < item.Count && item.TradeQuantity != 0)
              {
                  var givenItem = item.Clone();
                  givenItem.Count = item.TradeQuantity;

                  item.Count -= item.TradeQuantity;
                  item.TradeQuantity = 0;

                  _taskQueue.Enqueue(ActionType.UPDATE_ITEM_COUNT_IN_INVENTORY,
                                     Id, item.Bag, item.Slot, item.Count);

                  return givenItem;
              }

              _taskQueue.Enqueue(ActionType.REMOVE_ITEM_FROM_INVENTORY,
                                 Id, item.Bag, item.Slot);

              InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);

              if (item.ExpirationTime != null)
              {
                  item.StopExpirationTimer();
                  item.OnExpiration -= CharacterItem_OnExpiration;
              }

              _logger.LogDebug($"Character {Id} lost item {item.Type} {item.TypeId}");
              return item;*/
            return null;
        }

        #endregion
    }
}
