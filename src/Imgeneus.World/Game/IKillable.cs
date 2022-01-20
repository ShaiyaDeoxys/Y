using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Speed;
using System;

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

        /// <summary>
        /// Indicator, that shows if entity is dead or not.
        /// </summary>
        public bool IsDead { get; }

        /// <summary>
        /// Event, that is fired, when entity is killed.
        /// </summary>
        public event Action<IKillable, IKiller> OnDead;

        /// <summary>
        /// Absorbs damage regardless of REC value.
        /// </summary>
        public ushort Absorption { get; }

        /// <summary>
        /// Event, that is fired, when killable is resurrected.
        /// </summary>
        public event Action<IKillable> OnRebirthed;

        /// <summary>
        /// Resurrects killable to some coordinate.
        /// </summary>
        /// <param name="mapId">map id</param>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        public void Rebirth(ushort mapId, float x, float y, float z);

        /// <summary>
        /// Indicator, that shows if the entity cannot be damaged.
        /// </summary>
        public bool IsUntouchable { get; }
    }
}
