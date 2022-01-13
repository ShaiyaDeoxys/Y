using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelProvider : ILevelProvider
    {
        private readonly ILogger<LevelProvider> _logger;

        public LevelProvider(ILogger<LevelProvider> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug("LevelProvider {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~LevelProvider()
        {
            _logger.LogDebug("LevelProvider {hashcode} collected by GC", GetHashCode());
        }
#endif

        public ushort Level { get; set; }
    }
}
