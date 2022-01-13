using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player;
using MvvmHelpers;
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

        /// <summary>
        /// Element used in armor.
        /// </summary>
        public Element DefenceElement { get; }

        /// <summary>
        /// Element used in weapon.
        /// </summary>
        public Element AttackElement { get; }

        /// <summary>
        /// Indicator, that shows if entity is dead or not.
        /// </summary>
        public bool IsDead { get; }

        /// <summary>
        /// Event, that is fired, when entity is killed.
        /// </summary>
        public event Action<IKillable, IKiller> OnDead;

        /// <summary>
        /// Collection of current applied buffs.
        /// </summary>
        public ObservableRangeCollection<ActiveBuff> ActiveBuffs { get; }

        /// <summary>
        /// Updates collection of active buffs.
        /// </summary>
        public ActiveBuff AddActiveBuff(Skill skill, IKiller creator);

        /// <summary>
        /// Collection of current applied passive buffs.
        /// </summary>
        public ObservableRangeCollection<ActiveBuff> PassiveBuffs { get; }

        /// <summary>
        /// Current x position.
        /// </summary>
        public float PosX { get; }

        /// <summary>
        /// Current y position.
        /// </summary>
        public float PosY { get; }

        /// <summary>
        /// Current z position.
        /// </summary>
        public float PosZ { get; }

        /// <summary>
        /// Attack speed.
        /// </summary>
        public AttackSpeed AttackSpeed { get; }

        /// <summary>
        /// Move speed.
        /// </summary>
        public int MoveSpeed { get; }

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
