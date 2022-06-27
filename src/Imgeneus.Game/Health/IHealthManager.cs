using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Health
{
    /// <summary>
    /// Manages HP, MP, SP.
    /// </summary>
    public interface IHealthManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId, int currentHP, int currentSP, int currentMP, int? constHP = null, int? constSP = null, int? constMP = null);

        /// <summary>
        /// Max health.
        /// </summary>
        int MaxHP { get; }

        /// <summary>
        /// Max stamina.
        /// </summary>
        int MaxSP { get; }

        /// <summary>
        /// Max mana.
        /// </summary>
        int MaxMP { get; }

        /// <summary>
        /// Event, that is fired, when max hp changes.
        /// </summary>
        event Action<uint, int> OnMaxHPChanged;

        /// <summary>
        /// Event, that is fired, when max sp changes.
        /// </summary>
        event Action<uint, int> OnMaxSPChanged;

        /// <summary>
        /// Event, that is fired, when max mp changes.
        /// </summary>
        event Action<uint, int> OnMaxMPChanged;

        /// <summary>
        /// Health points based on level.
        /// </summary>
        int ConstHP { get; }

        /// <summary>
        /// Stamina points based on level.
        /// </summary>
        int ConstSP { get; }

        /// <summary>
        /// Mana points based on level.
        /// </summary>
        int ConstMP { get; }

        /// <summary>
        /// Health points, that are provided by equipment and buffs.
        /// </summary>
        int ExtraHP { get; set; }

        /// <summary>
        /// Stamina points, that are provided by equipment and buffs.
        /// </summary>
        int ExtraSP { get; set; }

        /// <summary>
        /// Mana points, that are provided by equipment and buffs.
        /// </summary>
        int ExtraMP { get; set; }

        /// <summary>
        /// Current health.
        /// </summary>
        int CurrentHP { get; }

        /// <summary>
        /// Current stamina.
        /// </summary>
        int CurrentSP { get; set; }

        /// <summary>
        /// Current mana.
        /// </summary>
        int CurrentMP { get; set; }

        /// <summary>
        /// Fires event, when there is some damage got from some killer.
        /// </summary>
        event Action<uint, IKiller, int> OnGotDamage;

        /// <summary>
        /// IKiller, that made max damage.
        /// </summary>
        IKiller MaxDamageMaker { get; }

        /// <summary>
        /// Decreases health and calculates how much damage was done in order to get who was killer later on.
        /// </summary>
        /// <param name="hp">damage hp</param>
        /// <param name="damageMaker">who has made damage</param>
        void DecreaseHP(int hp, IKiller damageMaker);

        /// <summary>
        /// Heals target hp.
        /// </summary>
        /// <param name="hp">hp healed</param>
        void IncreaseHP(int hp);

        /// <summary>
        /// Indicator, that shows if entity is dead or not.
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// Can be attacked by someone?
        /// </summary>
        bool IsAttackable { get; set; }

        /// <summary>
        /// Event, that is fired, when <see cref="IsAttackable"/> changes.
        /// </summary>
        event Action<bool> OnIsAttackableChanged;

        /// <summary>
        /// Event, that is fired, when entity is killed.
        /// </summary>
        event Action<uint, IKiller> OnDead;

        /// <summary>
        /// Event, that is fired, when killable is resurrected.
        /// </summary>
        event Action<uint> OnRebirthed;

        /// <summary>
        /// Resurrects killable.
        /// </summary>
        void Rebirth();

        /// <summary>
        /// Recoves all 3 stats at once.
        /// </summary>
        void Recover(int hp, int mp, int sp);

        /// <summary>
        /// Fully recovers all hitpoints.
        /// </summary>
        void FullRecover();

        /// <summary>
        /// Event, that is fired, when hp changes.
        /// </summary>
        event Action<uint, HitpointArgs> HP_Changed;

        /// <summary>
        /// Event, that is fired, when mp changes.
        /// </summary>
        event Action<uint, HitpointArgs> MP_Changed;

        /// <summary>
        /// Event, that is fired, when sp changes.
        /// </summary>
        event Action<uint, HitpointArgs> SP_Changed;

        /// <summary>
        /// Event, that is fired, when player used any skill and both MP and SP changed.
        /// </summary>
        event Action<ushort, ushort> MP_SP_Used;

        /// <summary>
        /// Fires <see cref="MP_SP_Used"/>
        /// </summary>
        void InvokeUsedMPSP(ushort usedMP, ushort usedSP);

        /// <summary>
        /// Event, that is fired, when killable recovers.
        /// </summary>
        event Action<uint, int, int, int> OnRecover;

        /// <summary>
        /// Raises all 3 hitpoints change.
        /// </summary>
        void RaiseHitpointsChange();

        /// <summary>
        /// Event, that is fired, when all 3 hitpoints change.
        /// </summary>
        event Action<int, int, int> OnCurrentHitpointsChanged;

        /// <summary>
        /// Reflects all physical damage.
        /// </summary>
        bool ReflectPhysicDamage { get; set; }

        /// <summary>
        /// Reflects all magical damage.
        /// </summary>
        bool ReflectMagicDamage { get; set; }

        /// <summary>
        /// Makes mirrow damage.
        /// </summary>
        void InvokeMirrowDamage(Damage damage, IKillable damageMaker);

        /// <summary>
        /// Event, that is fired when mirror damage comes.
        /// </summary>
        event Action<uint, uint, Damage> OnMirrowDamage;

        /// <summary>
        /// Use MP before HP.
        /// </summary>
        bool UseMPInsteadOfHP { get; set; }

        /// <summary>
        /// Use SP before HP.
        /// </summary>
        bool UseSPInsteadOfHP { get; set; }
    }
}
