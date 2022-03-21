using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Untouchable
{
    public class UntouchableManager : IUntouchableManager
    {
        private readonly ILogger<UntouchableManager> _logger;

        private int _ownerId;

        public UntouchableManager(ILogger<UntouchableManager> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("UntouchableManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~UntouchableManager()
        {
            _logger.LogDebug("UntouchableManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        public bool IsUntouchable { get; set; }
    }
}
