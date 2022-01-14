using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Skills;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// Special interface, that all killers must implement.
    /// Killer can be another player, npc or mob.
    /// </summary>
    public interface IKiller : IWorldMember, IStatsHolder
    {
        public ILevelProvider LevelProvider { get; }

        public IAttackManager AttackManager { get; }

        public ISkillsManager SkillsManager { get; }

        /// <summary>
        /// Killer's fraction.
        /// </summary>
        Fraction Country { get; }
    }
}
