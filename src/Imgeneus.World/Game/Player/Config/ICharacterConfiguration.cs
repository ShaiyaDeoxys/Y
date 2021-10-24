using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Player.Config
{
    public interface ICharacterConfiguration
    {
        /// <summary>
        /// Config for each job and level.
        /// </summary>
        public Character_HP_SP_MP[] Configs { get; }

        /// <summary>
        /// Default starting stats.
        /// </summary>
        public DefaultStat[] DefaultStats { get; set; }

        /// <summary>
        /// Default maximum level for each mode
        /// </summary>
        public DefaultMaxLevel[] DefaultMaxLevels { get; set; }

        /// <summary>
        /// Default stat and skill points received per level
        /// </summary>
        public DefaultLevelStatSkillPoints[] DefaultLevelStatSkillPoints { get; set; }

        public Character_HP_SP_MP GetConfig(int index);

        public DefaultMaxLevel GetMaxLevelConfig(Mode mode);

        public DefaultLevelStatSkillPoints GetLevelStatSkillPoints(Mode mode);

        /// <summary>
        /// Start position and items for each faction and job, when character is created.
        /// </summary>
        public CreationConfiguration[] CreateConfigs { get; set; }
    }
}
