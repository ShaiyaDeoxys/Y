using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Inventory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Dyeing
{
    public class DyeingManager : IDyeingManager
    {
        private readonly ILogger<DyeingManager> _logger;
        private readonly IInventoryManager _inventoryManager;

        public DyeingManager(ILogger<DyeingManager> logger, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;

#if DEBUG
            _logger.LogDebug("DyeingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~DyeingManager()
        {
            _logger.LogDebug("DyeingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public List<DyeColor> AvailableColors { get; private set; } = new List<DyeColor>();

        private Dictionary<int, Color> _colors = new Dictionary<int, Color>()
        {
            { 0, Color.White },
            { 1, Color.Black },
            { 2, Color.Red },
            { 3, Color.Orange },
            { 4, Color.Yellow },
            { 5, Color.LightBlue },
            { 6, Color.Blue },
            { 7, Color.Green },
            { 8, Color.DarkGreen },
            { 9, Color.Azure },
            { 10, Color.Violet },
            { 11, Color.PaleVioletRed },
            { 12, Color.Brown },
            { 13, Color.DeepPink },
            { 14, Color.Gray },
            { 15, Color.LightSeaGreen },
            { 16, Color.MintCream },
            { 17, Color.YellowGreen },
            { 18, Color.OrangeRed },
            { 19, Color.Chocolate },
            { 20, Color.Crimson },
        };

        private Random _random = new Random();

        public void Reroll()
        {
            AvailableColors.Clear();

            // Always generate 5 random colors.
            Color color;
            byte saturation = 35;

            color = _colors[_random.Next(0, _colors.Keys.Count)];
            AvailableColors.Add(new DyeColor(200, saturation, color.R, color.G, color.B));

            color = _colors[_random.Next(0, _colors.Keys.Count)];
            saturation += 15;
            AvailableColors.Add(new DyeColor(200, saturation, color.R, color.G, color.B));

            color = _colors[_random.Next(0, _colors.Keys.Count)];
            saturation += 50;
            AvailableColors.Add(new DyeColor(200, saturation, color.R, color.G, color.B));

            color = _colors[_random.Next(0, _colors.Keys.Count)];
            saturation += 50;
            AvailableColors.Add(new DyeColor(200, saturation, color.R, color.G, color.B));

            color = _colors[_random.Next(0, _colors.Keys.Count)];
            saturation += 50;
            AvailableColors.Add(new DyeColor(200, saturation, color.R, color.G, color.B));
        }

        public bool SelectItem(byte dyeItemBag, byte dyeItemSlot, byte targetItemBag, byte targetItemSlot)
        {
            _inventoryManager.InventoryItems.TryGetValue((dyeItemBag, dyeItemSlot), out var dyeItem);
            if (dyeItem is null || dyeItem.Special != SpecialEffect.Dye)
                return false;

            _inventoryManager.InventoryItems.TryGetValue((targetItemBag, targetItemSlot), out var item);
            if (item is null)
                return false;

            return CanDye(dyeItem, item);
        }

        public async Task<(bool Ok, DyeColor Color)> Dye(byte dyeItemBag, byte dyeItemSlot, byte targetItemBag, byte targetItemSlot)
        {
            if (AvailableColors.Count == 0)
                Reroll();

            var color = AvailableColors[new Random().Next(0, 5)];

            _inventoryManager.InventoryItems.TryGetValue((dyeItemBag, dyeItemSlot), out var dyeItem);
            if (dyeItem is null || dyeItem.Special != SpecialEffect.Dye)
            {
                return (false, color);
            }

            _inventoryManager.InventoryItems.TryGetValue((targetItemBag, targetItemSlot), out var item);
            if (item is null)
            {
                return (false, color);
            }

            bool success = CanDye(dyeItem, item);

            if (success)
            {
                item.DyeColor = color;

                await _inventoryManager.TryUseItem(dyeItem.Bag, dyeItem.Slot, skipApplyingItemEffect: true);

                return (success, color);
            }
            else
            {
                return (false, color);
            }
        }

        private bool CanDye(Item dyeItem, Item targetItem)
        {
            return (dyeItem.TypeId == 55 && targetItem.IsWeapon) ||
                   (dyeItem.TypeId == 56 && targetItem.IsArmor) ||
                   (dyeItem.TypeId == 57 && targetItem.IsMount) ||
                   (dyeItem.TypeId == 58 && targetItem.IsPet) ||
                   (dyeItem.TypeId == 59 && targetItem.IsCostume) ||
                   (dyeItem.TypeId == 60 && targetItem.IsWing);
        }
    }
}
