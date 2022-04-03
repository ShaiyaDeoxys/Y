using Imgeneus.World.Game.Inventory;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Linking
{
    /// <summary>
    /// Adds/removes gems, lapisia, rec rune etc.
    /// </summary>
    public interface ILinkingManager
    {
        /// <summary>
        /// Tries to add gem to item.
        /// </summary>
        /// <returns>true, if gem was successfully linked, otherwise false; also returns slot, where gem was linked</returns>
        (bool Success, byte Slot, Item Gem, Item Item, Item Hammer) AddGem(byte bag, byte slot, byte destinationBag, byte destinationSlot, byte hammerBag, byte hammerSlot);

        /// <summary>
        /// Removes gem from item.
        /// </summary>
        (bool Success, byte Slot, List<Item> SavedGems, Item Item) RemoveGem(byte bag, byte slot, bool shouldRemoveSpecificGem, byte gemPosition, byte hammerBag, byte hammerSlot);

        /// <summary>
        /// Gets success rate based on gem and hammer(if presented).
        /// </summary>
        double GetRate(Item gem, Item hammer);

        /// <summary>
        /// Gets success rate of removing gem based on gem and hammer(if presented).
        /// </summary>
        /// <param name="extraRate">extra rate, that doesn't depend on gem or hammer. E.g. guild house blacksmith or bless rate</param>
        double GetRemoveRate(Gem gem, Item hammer, byte extraRate = 0);

        /// <summary>
        /// Gets gold amount for linking based on gem.
        /// </summary>
        int GetGold(Item gem);

        /// <summary>
        /// Gets gold amount for extracting based on gem.
        /// </summary>
        int GetRemoveGold(Gem gem);

        /// <summary>
        /// 3 random stats.
        /// </summary>
        (bool Success, Item Item) Compose(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot);

        /// <summary>
        /// ?
        /// </summary>
        (bool Success, Item Item) AbsoluteCompose(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot);

        /// <summary>
        /// Calculates enchantment rate of specified item.
        /// </summary>
        uint GetEnchantmentRate(Item item);

        /// <summary>
        /// Calculates enchantment cost for specified item.
        /// </summary>
        uint GetEnchantmentGold(Item item);
    }
}
