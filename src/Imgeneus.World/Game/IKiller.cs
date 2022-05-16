using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// Special interface, that all killers must implement.
    /// Killer can be another player, npc or mob.
    /// </summary>
    public interface IKiller : IWorldMember, IStatsHolder
    {
        public ILevelProvider LevelProvider { get; }

        public ISpeedManager SpeedManager { get; }

        public IAttackManager AttackManager { get; }

        public ISkillsManager SkillsManager { get; }

        public IMovementManager MovementManager { get; }
    }
}
