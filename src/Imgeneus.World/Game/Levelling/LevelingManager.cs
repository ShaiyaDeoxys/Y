using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Player.Config;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelingManager : ILevelingManager
    {
        private readonly ILogger<LevelingManager> _logger;
        private readonly ICharacterConfiguration _characterConfiguration;

        public LevelingManager(ILogger<LevelingManager> logger, ICharacterConfiguration characterConfiguration)
        {
            _logger = logger;
            _characterConfiguration = characterConfiguration;

#if DEBUG
            _logger.LogDebug($"LevelingManager {GetHashCode()} created");
#endif
        }

#if DEBUG
        ~LevelingManager()
        {
            _logger.LogDebug($"LevelingManager {GetHashCode()} collected by GC");
        }
#endif

        public void Init(ushort level, CharacterProfession? profession = null, int? constHP = null, int? constSP = null, int? constMP = null)
        {
            Level = level;

            Class = profession;

            _constHP = constHP;
            _constSP = constSP;
            _constMP = constMP;
        }

        public ushort Level { get; set; }

        public CharacterProfession? Class { get; private set; }

        private int? _constHP;
        public int ConstHP
        {
            get
            {
                if (_constHP.HasValue)
                    return _constHP.Value;

                var level = Level <= 60 ? Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constHP = _characterConfiguration.GetConfig(index).HP;

                return constHP;

            }
            private set => _constHP = value;
        }


        private int? _constSP;
        public int ConstSP {
            get
            {
                if (_constSP.HasValue)
                    return _constSP.Value;

                var level = Level <= 60 ? Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constSP = _characterConfiguration.GetConfig(index).SP;

                return constSP;
            }

            private set => _constSP = value;
        }


        private int? _constMP;
        public int ConstMP {

            get
            {
                if (_constMP.HasValue)
                    return _constMP.Value;

                var level = Level <= 60 ? Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constMP = _characterConfiguration.GetConfig(index).MP;

                return constMP;
            }

            private set => _constMP = value;
        }

    }
}
