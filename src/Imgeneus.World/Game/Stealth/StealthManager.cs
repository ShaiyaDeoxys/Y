using Imgeneus.World.Game.Session;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Stealth
{
    public class StealthManager : IStealthManager
    {
        private readonly ILogger<StealthManager> _logger;
        private readonly IGameSession _gameSession;

        public event Action<int> OnStealthChange;

        public StealthManager(ILogger<StealthManager> logger, IGameSession gameSession)
        {
            _logger = logger;
            _gameSession = gameSession;

#if DEBUG
            _logger.LogDebug($"StealthManager {GetHashCode()} created");
#endif
        }

#if DEBUG
        ~StealthManager()
        {
            _logger.LogDebug($"StealthManager {GetHashCode()} collected by GC");
        }
#endif

        private bool _isAdminStealth = false;
        public bool IsAdminStealth
        {
            set
            {
                if (!_gameSession.IsAdmin || _isAdminStealth == value)
                    return;

                _isAdminStealth = value;

                OnStealthChange?.Invoke(_gameSession.CharId);
                //InvokeAttackOrMoveChanged();
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

                OnStealthChange?.Invoke(_gameSession.CharId);
                //SendRunMode(); // Do we need this in new eps?
                //InvokeAttackOrMoveChanged();
            }
            get => _isStealth || _isAdminStealth;
        }
    }
}
