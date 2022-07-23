using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Inventory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Imgeneus.Game.Crafting
{
    public class CraftingManager : ICraftingManager
    {
        private readonly Random _random = new Random();

        private readonly ILogger<CraftingManager> _logger;
        private readonly IInventoryManager _inventoryManager;
        private readonly ICraftingConfiguration _craftingConfiguration;

        public CraftingManager(ILogger<CraftingManager> logger, IInventoryManager inventoryManager, ICraftingConfiguration craftingConfiguration)
        {
            _logger = logger;
            _inventoryManager = inventoryManager;
            _craftingConfiguration = craftingConfiguration;
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

            var rate = Math.Round(recipe.Rate + (hammer is not null ? hammer.LinkingRate : 0));
            var success = rate >= _random.Next(1, 100);
            return success;
        }
    }
}
