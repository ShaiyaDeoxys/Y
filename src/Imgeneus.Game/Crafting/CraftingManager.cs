using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.Game.Crafting
{
    public class CraftingManager : ICraftingManager
    {
        private readonly Random _random = new Random();

        private readonly ILogger<CraftingManager> _logger;
        private readonly IInventoryManager _inventoryManager;
        private readonly ICraftingConfiguration _craftingConfiguration;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IItemEnchantConfiguration _enchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;

        public CraftingManager(ILogger<CraftingManager> logger, IInventoryManager inventoryManager, ICraftingConfiguration craftingConfiguration, IGameDefinitionsPreloder definitionsPreloader, IItemEnchantConfiguration enchantConfig, IItemCreateConfiguration itemCreateConfig)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;
            _craftingConfiguration = craftingConfiguration;
            _definitionsPreloader = definitionsPreloader;
            _enchantConfig = enchantConfig;
            _itemCreateConfig = itemCreateConfig;
#if DEBUG
            _logger.LogDebug("CraftingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~CraftingManager()
        {
            _logger.LogDebug("CraftingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public (byte Type, byte TypeId) ChaoticSquare { get; set; }

        public bool TryCraft(byte bag, byte slot, int index, byte hammerBag, byte hammerSlot)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((bag, slot), out var craftSquare))
                return false;

            if (craftSquare.Special != SpecialEffect.ChaoticSquare)
                return false;

            var config = _craftingConfiguration.SquareItems.FirstOrDefault(x => x.Type == craftSquare.Type && x.TypeId == craftSquare.TypeId);
            if (config is null || config.Recipes.Count < index)
                return false;

            Item hammer = null;
            if (hammerBag != 0)
                _inventoryManager.InventoryItems.TryGetValue((hammerBag, hammerSlot), out hammer);

            if (hammer != null && hammer.Special != SpecialEffect.CraftingHammer)
                hammer = null;

            var recipe = config.Recipes[index];
            var useIngredients = new List<(Ingredient Ingredient, Item Item)>();

            foreach (var ingredient in recipe.Ingredients)
            {
                var useItem = _inventoryManager.InventoryItems.Values.FirstOrDefault(x => x.Type == ingredient.Type && x.TypeId == ingredient.TypeId && x.Count >= ingredient.Count);
                if (useItem is null)
                    return false;

                useIngredients.Add((ingredient, useItem));
            }

            foreach (var x in useIngredients)
                _inventoryManager.TryUseItem(x.Item.Bag, x.Item.Slot, skipApplyingItemEffect: true, count: x.Ingredient.Count);

            if (hammer is not null)
                _inventoryManager.TryUseItem(hammer.Bag, hammer.Slot, skipApplyingItemEffect: true);

            _inventoryManager.TryUseItem(craftSquare.Bag, craftSquare.Slot, skipApplyingItemEffect: true);
            
            var rate = Math.Round(recipe.Rate + (hammer is not null ? hammer.LinkingRate : 0));
            var success = rate >= _random.Next(1, 100);

            if (success)
                _inventoryManager.AddItem(new Item(_definitionsPreloader, _enchantConfig, _itemCreateConfig, recipe.Type, recipe.TypeId, recipe.Count), "crafting");

            return success;
        }
    }
}
