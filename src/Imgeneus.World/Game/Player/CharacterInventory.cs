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

        #region Use Item

          




        

        #endregion

        #region Buy/sell Item

        /// <summary>
        /// Sells item.
        /// </summary>
        /// <param name="item">item to sell</param>
        /// <param name="count">how many item player want to sell</param>
        public Item SellItem(Item item, byte count)
        {
            if (!InventoryItems.ContainsKey((item.Bag, item.Slot)))
            {
                return null;
            }

            item.TradeQuantity = count > item.Count ? item.Count : count;
            InventoryManager.Gold = (uint)(InventoryManager.Gold + item.Sell * item.TradeQuantity);
            return RemoveItemFromInventory(item);
        }

        #endregion

        #region Item Expiration

        public void CharacterItem_OnExpiration(Item item)
        {
            RemoveItemFromInventory(item);
            SendRemoveItemFromInventory(item, true);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Tries to find free slot in inventory.
        /// </summary>
        /// <returns>tuple of bag and slot; slot is -1 if there is no free slot</returns>
        private (byte Bag, int Slot) FindFreeSlotInInventory()
        {
            byte bagSlot = 0;
            int freeSlot = -1;

            if (InventoryItems.Count > 0)
            {
                var maxBag = 5;
                var maxSlots = 24;

                // Go though all bags and try to find any free slot.
                // Start with 1, because 0 is worn items.
                for (byte i = 1; i <= maxBag; i++)
                {
                    var bagItems = InventoryItems.Where(itm => itm.Value.Bag == i).OrderBy(b => b.Value.Slot);
                    for (var j = 0; j < maxSlots; j++)
                    {
                        if (!bagItems.Any(b => b.Value.Slot == j))
                        {
                            freeSlot = j;
                            break;
                        }
                    }

                    if (freeSlot != -1)
                    {
                        bagSlot = i;
                        break;
                    }
                }
            }
            else
            {
                bagSlot = 1; // Start with 1, because 0 is worn items.
                freeSlot = 0;
            }

            return (bagSlot, freeSlot);
        }


        #endregion
    }
}
