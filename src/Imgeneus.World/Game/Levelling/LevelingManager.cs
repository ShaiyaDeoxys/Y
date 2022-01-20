using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelingManager : ILevelingManager
    {
        private readonly ILogger<LevelingManager> _logger;
        private readonly IDatabase _database;
        private readonly IGameSession _gameSession;
        private readonly ILevelProvider _levelProvider;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly IStatsManager _statsManager;
        private int _owner;

        public LevelingManager(ILogger<LevelingManager> logger, IDatabase database, IGameSession gameSession, ILevelProvider levelProvider, IAdditionalInfoManager additionalInfoManager, IStatsManager statsManager)
        {
            _logger = logger;
            _database = database;
            _gameSession = gameSession;
            _levelProvider = levelProvider;
            _additionalInfoManager = additionalInfoManager;
            _statsManager = statsManager;

#if DEBUG
            _logger.LogDebug("LevelingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~LevelingManager()
        {
            _logger.LogDebug("LevelingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(int owner, Mode grow)
        {
            _owner = owner;
            Grow = grow;
        }

        #region Grow

        public Mode Grow { get; private set; }

        public async Task<bool> TrySetGrow(Mode grow)
        {
            if (Grow == grow)
                return true;

            if (grow > Mode.Ultimate)
                return false;

            var character = await _database.Characters.FindAsync(_gameSession.CharId);
            if (character is null)
                return false;

            character.Mode = grow;

            var ok = (await _database.SaveChangesAsync()) > 0;
            if (ok)
                Grow = grow;

            return ok;
        }

        #endregion

        #region Primary stats


        /// <summary>
        /// Increases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        public async Task IncreasePrimaryStat(ushort amount = 1)
        {
            var primaryAttribute = _additionalInfoManager.GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    await _statsManager.TrySetStats(str: (ushort)(_statsManager.Strength + amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    await _statsManager.TrySetStats(dex: (ushort)(_statsManager.Dexterity + amount));
                    break;

                case CharacterStatEnum.Reaction:
                    await _statsManager.TrySetStats(rec: (ushort)(_statsManager.Reaction + amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    await _statsManager.TrySetStats(intl: (ushort)(_statsManager.Intelligence + amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    await _statsManager.TrySetStats(wis: (ushort)(_statsManager.Wisdom + amount));
                    break;

                case CharacterStatEnum.Luck:
                    await _statsManager.TrySetStats(luc: (ushort)(_statsManager.Luck + amount));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Decreases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        public async Task DecreasePrimaryStat(ushort amount = 1)
        {
            var primaryAttribute = _additionalInfoManager.GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    await _statsManager.TrySetStats(str: (ushort)(_statsManager.Strength - amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    await _statsManager.TrySetStats(dex: (ushort)(_statsManager.Dexterity - amount));
                    break;

                case CharacterStatEnum.Reaction:
                    await _statsManager.TrySetStats(rec: (ushort)(_statsManager.Reaction - amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    await _statsManager.TrySetStats(intl: (ushort)(_statsManager.Intelligence - amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    await _statsManager.TrySetStats(wis: (ushort)(_statsManager.Wisdom - amount));
                    break;

                case CharacterStatEnum.Luck:
                    await _statsManager.TrySetStats(luc: (ushort)(_statsManager.Luck - amount));
                    break;

                default:
                    break;
            }
        }

        #endregion
    }
}
