using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelingManager : ILevelingManager
    {
        private readonly ILogger<LevelingManager> _logger;

        public LevelingManager(ILogger<LevelingManager> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug("LevelingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~LevelingManager()
        {
            _logger.LogDebug("LevelingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init()
        {
        }

    }
}
