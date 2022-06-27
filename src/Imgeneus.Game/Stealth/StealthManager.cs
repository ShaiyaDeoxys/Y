using Imgeneus.World.Game.Health;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Stealth
{
    public class StealthManager : IStealthManager
    {
        private readonly ILogger<StealthManager> _logger;

        private uint _ownerId;

        public StealthManager(ILogger<StealthManager> logger)
        {
            _logger = logger;

#if DEBUG
            _logger.LogDebug("StealthManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~StealthManager()
        {
            _logger.LogDebug("StealthManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear
        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        #endregion

        public event Action<uint> OnStealthChange;

        private bool _isAdminStealth = false;
        public bool IsAdminStealth
        {
            set
            {
                if (_isAdminStealth == value)
                    return;

                _isAdminStealth = value;

                OnStealthChange?.Invoke(_ownerId);
            }

            get => _isAdminStealth;
        }

        private bool _isStealth = false;
        public bool IsStealth
        {
            set
            {
                if (_isStealth == value)
                    return;

                _isStealth = value;

                OnStealthChange?.Invoke(_ownerId);
                //SendRunMode(); // Do we need this in new eps?
            }
            get => _isStealth || _isAdminStealth;
        }
    }
}
