using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelProvider : ILevelProvider
    {
        private readonly ILogger<LevelProvider> _logger;
        private uint _ownerId;

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

        #region Init & Clear

        public void Init(uint ownerId, ushort level)
        {
            _ownerId = ownerId;
            _level = level;
        }

        #endregion

        private ushort _level;
        public ushort Level
        {
            get => _level; 
            set
            {
                var oldLevel = value;
                _level = value;
                OnLevelUp?.Invoke(_ownerId, _level, oldLevel);
            }
        }

        public event Action<uint, ushort, ushort> OnLevelUp;
    }
}
