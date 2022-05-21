using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Zone
{
    public class MapProvider : IMapProvider
    {
        private readonly ILogger<MapProvider> _logger;

        public MapProvider(ILogger<MapProvider> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("MapProvider {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~MapProvider()
        {
            _logger.LogDebug("MapProvider {hashcode} collected by GC", GetHashCode());
        }
#endif

        public Map Map { get; set; }
        public ushort NextMapId { get; set; }
        public int CellId { get; set; } = -1;
        public int OldCellId { get; set; } = -1;
    }
}
