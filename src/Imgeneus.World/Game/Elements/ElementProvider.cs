using Imgeneus.Database.Constants;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Elements
{
    public class ElementProvider : IElementProvider
    {
        private readonly ILogger<ElementProvider> _logger;

        public ElementProvider(ILogger<ElementProvider> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug("ElementProvider {hashcode} created", GetHashCode());
            _logger = logger;
#endif
        }

#if DEBUG
        ~ElementProvider()
        {
            _logger.LogDebug("ElementProvider {hashcode} collected by GC", GetHashCode());
        }
#endif

        public Element AttackElement { get; set; }

        public Element DefenceElement { get; set; }
    }
}
