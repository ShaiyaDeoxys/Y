using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Levelling
{
    public class LevelingManager : ILevelingManager
    {
        private readonly ILogger<LevelingManager> _logger;
        private readonly IDatabase _database;
        private readonly ILevelProvider _levelProvider;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly ICharacterConfiguration _characterConfig;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IPartyManager _partyManager;
        private readonly IMapProvider _mapProvider;
        private readonly IMovementManager _movementManager;
        private uint _ownerId;

        public LevelingManager(ILogger<LevelingManager> logger, IDatabase database, ILevelProvider levelProvider, IAdditionalInfoManager additionalInfoManager, ICharacterConfiguration charConfig, IDatabasePreloader databasePreloader, IPartyManager partyManager, IMapProvider mapProvider, IMovementManager movementManager)
        {
            _logger = logger;
            _database = database;
            _levelProvider = levelProvider;
            _additionalInfoManager = additionalInfoManager;
            _characterConfig = charConfig;
            _databasePreloader = databasePreloader;
            _partyManager = partyManager;
            _mapProvider = mapProvider;
            _movementManager = movementManager;

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

        public void Init(uint ownerId, uint exp)
        {
            _ownerId = ownerId;

            Exp = exp;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                return;

            character.Exp = Exp;
            character.Level = _levelProvider.Level;

            await _database.SaveChangesAsync();
        }

        #region Leveling

        private uint _exp;
        public uint Exp
        {
            get => _exp;
            private set
            {
                var oldValue = _exp;
                _exp = value;

                OnExpChanged?.Invoke(_exp - oldValue);
            }
        }

        public event Action<uint> OnExpChanged;

        public uint ExpGainRate { get; set; }

        public uint MinLevelExp => _levelProvider.Level > 1 ? _databasePreloader.Levels[(_additionalInfoManager.Grow, (ushort)(_levelProvider.Level - 1))].Exp : 0;

        public uint NextLevelExp => _databasePreloader.Levels[(_additionalInfoManager.Grow, _levelProvider.Level)].Exp;

        public bool TryChangeLevel(ushort newLevel)
        {
            // Set character's new level
            if (!CanSetLevel(newLevel))
                return false;

            _levelProvider.Level = newLevel;

            // Check that experience is at least the minimum experience for the level, without notifiyng client.
            if (_exp < MinLevelExp || _exp >= NextLevelExp)
                // Change player experience to 0% of current level
                _exp = MinLevelExp;

            return true;
        }

        /// <summary>
        /// Checks if level can be set based on some character configs.
        /// </summary>
        private bool CanSetLevel(ushort newLevel)
        {
            if (_levelProvider.Level == newLevel)
                return false;

            // Check minimum level boundary
            if (newLevel < 1)
                return false;

            // Check maximum level boundary
            var maxLevel = _characterConfig.GetMaxLevelConfig(_additionalInfoManager.Grow).Level;

            if (newLevel > maxLevel)
                return false;

            return true;
        }

        public bool TryChangeExperience(uint exp)
        {
            // TODO: Multiply exp by global exp multiplier

            // Round exp to nearest multiple of 10
            exp = MathExtensions.RoundToTenMultiple(exp);

            // Validate the new experience value
            if (!CanSetExperience(exp) || Exp == exp)
                return false;

            Exp = exp;

            // Check current level experience boundaries and change level if necessary
            if (Exp < MinLevelExp || Exp >= NextLevelExp)
                // Update level to value that matches the new experience value
                TryChangeLevel(LevelByExperience);

            return true;
        }

        public void AddMobExperience(ushort mobLevel, ushort mobExp)
        {
            if (_partyManager.HasParty)
            {
                var partyMemberCount = _partyManager.Party.Members.Count;

                ushort memberExp = 0;

                // If there are 7 party members, party is perfect party and experience is given as if there were only 2 party members
                if (partyMemberCount == 7)
                    memberExp = (ushort)(mobExp / 2);
                else
                    memberExp = (ushort)(mobExp / partyMemberCount);

                // Get party members who are near the player who got experience
                var nearbyPartyMembers = _partyManager.Party.Members.Where(m => m.Map == _mapProvider.Map &&
                                                                 MathExtensions.Distance(_movementManager.PosX, m.PosX, _movementManager.PosZ, m.PosZ) < 50);

                // Give experience to every party member
                foreach (var partyMember in nearbyPartyMembers)
                {
                    var exp = CalculateExperienceFromMob(mobLevel, memberExp, partyMember.LevelProvider.Level);
                    if (exp == 0)
                        continue;

                    partyMember.LevelingManager.TryChangeExperience(partyMember.LevelingManager.Exp + exp);
                }
            }
            else
            {
                var exp = CalculateExperienceFromMob(mobLevel, mobExp, _levelProvider.Level);
                if (exp == 0)
                    return;

                TryChangeExperience(Exp + exp);
            }
        }

        /// <summary>
        /// Helper method that calculates the level that corresponds to a certain experience value.
        /// </summary>
        private ushort LevelByExperience
        {
            get
            {
                var levelInfo = _databasePreloader.Levels.Values
                .Where(l => l.Exp > Exp)
                .OrderBy(l => l.Level)
                .First();

                return levelInfo.Level;
            }
        }

        /// <summary>
        /// Checks if an experience value can be set, verifying it doesn't exceed the max level's experience
        /// </summary>
        /// <param name="exp"></param>
        /// <returns>Success status indicating whether it is possible to set an experience value or not.</returns>
        private bool CanSetExperience(uint exp)
        {
            // Get max level from config file
            var maxLevel = _characterConfig.GetMaxLevelConfig(_additionalInfoManager.Grow).Level;

            // Get max level info
            var maxLevelInfo = _databasePreloader.Levels[(_additionalInfoManager.Grow, maxLevel)];

            // Exp can't be superior than max level's experience
            return exp <= maxLevelInfo.Exp;
        }

        /// <summary>
        /// Calculates the experience a player should get from killing a mob based on his level.
        /// </summary>
        /// <param name="mobLevel">Killed mob's level</param>
        /// <param name="mobExp">Killed mob's experience</param>
        /// <returns>Experience value</returns>
        private ushort CalculateExperienceFromMob(ushort mobLevel, ushort mobExp, ushort characterLevel)
        {
            var levelDifference = characterLevel - mobLevel;

            // Character can't get experience from mob that's more than 8 levels above him or more than 6 levels below him
            if (levelDifference < -8 || levelDifference > 6)
                return 0;

            // Calculate experience based on exp formula
            var exp = (ushort)((-24 * levelDifference + 167) / 100f * mobExp);

            if (ExpGainRate > 0)
                exp = (ushort)(exp * ExpGainRate / 100);

            return exp;
        }

        #endregion
    }
}
