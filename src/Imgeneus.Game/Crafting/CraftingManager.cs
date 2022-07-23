using Microsoft.Extensions.Logging;

namespace Imgeneus.Game.Crafting
{
    public class CraftingManager : ICraftingManager
    {
        private readonly ILogger<CraftingManager> _logger;

        public CraftingManager(ILogger<CraftingManager> logger)
        {
            _logger = logger;
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
    }
}
