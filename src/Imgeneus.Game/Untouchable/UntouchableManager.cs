using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Untouchable
{
    public class UntouchableManager : IUntouchableManager
    {
        private readonly ILogger<UntouchableManager> _logger;

        private uint _ownerId;

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

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        public bool IsUntouchable { get; set; }

        private byte _blockedMagicAttacks;
        public byte BlockedMagicAttacks
        {
            get => _blockedMagicAttacks;
            set
            {
                _blockedMagicAttacks = value;
                OnBlockedMagicAttacksChanged?.Invoke(_blockedMagicAttacks);
            }
        }

        public event Action<byte> OnBlockedMagicAttacksChanged;

        public bool BlockDebuffs { get; set; }
    }
}
