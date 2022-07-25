using Imgeneus.Database;
using Microsoft.Extensions.Logging;

namespace Imgeneus.Game.Market
{
    public class MarketManager : IMarketManager
    {
        private readonly ILogger<MarketManager> _logger;

        private uint _ownerId;

        public MarketManager(ILogger<MarketManager> logger, IDatabase database)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("MarketManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~MarketManager()
        {
            _logger.LogDebug("MarketManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public bool TryRegisterItem(byte bag, byte slot, byte count, MarketType marketType, uint minMoney, uint directMoney)
        {
            return false;
        }
    }
}
