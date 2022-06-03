using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Zone;

namespace Imgeneus.World.Game
{

    /// <summary>
    /// Special interface, that all killable objects must implement.
    /// Killable objects like: players, mobs.
    /// </summary>
    public interface IKillable : IWorldMember, IStatsHolder
    {
        public IHealthManager HealthManager { get; }

        public ILevelProvider LevelProvider { get; }

        public IBuffsManager BuffsManager { get; }

        public IElementProvider ElementProvider { get; }

        public IMovementManager MovementManager { get; }

        public IMapProvider MapProvider { get; }

        public IUntouchableManager UntouchableManager { get; }

        /// <summary>
        /// Absorbs damage regardless of REC value.
        /// </summary>
        public ushort Absorption { get; }
    }
}
