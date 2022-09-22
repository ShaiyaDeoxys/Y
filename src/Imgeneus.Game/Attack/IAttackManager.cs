using Imgeneus.Database.Constants;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.World.Game.Buffs;
using System;
using System.Collections.Generic;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Game.Attack
{
    public interface IAttackManager
    {
        /// <summary>
        /// Number for autoattack.
        /// </summary>
        public const byte AUTO_ATTACK_NUMBER = 255;

        /// <summary>
        /// Inits attack manager.
        /// </summary>
        void Init(uint ownerId);

        /// <summary>
        /// If this set to true, attack will always success. Used mainly in tests.
        /// </summary>
        bool AlwaysHit { get; set; }

        /// <summary>
        /// Updates the last date, when attack was called.
        /// </summary>
        void StartAttack();

        /// <summary>
        /// Event, that is fired when <see cref="Target"/> changes.
        /// </summary>
        event Action<IKillable> OnTargetChanged;

        /// <summary>
        /// Current enemy in target.
        /// </summary>
        IKillable Target { get; set; }

        event Action<IKillable, Buff> TargetOnBuffAdded;

        event Action<IKillable, Buff> TargetOnBuffRemoved;

        /// <summary>
        /// Set by inventory weapon.
        /// </summary>
        bool IsWeaponAvailable { get; set; }

        /// <summary>
        /// Set by inventory shield.
        /// </summary>
        bool IsShieldAvailable { get; set; }

        /// <summary>
        /// Checks if it's possible to attack target. (or use skill)
        /// </summary>
        bool CanAttack(byte skillNumber, IKillable target, out AttackSuccess success);

        /// <summary>
        /// Usual physical attack, "auto attack".
        /// </summary>
        void AutoAttack(IKiller sender);

        /// <summary>
        /// Event before each attack.
        /// </summary>
        event Action OnStartAttack;

        /// <summary>
        /// Event, that is fired, when melee attack.
        /// </summary>
        event Action<uint, IKillable, AttackResult> OnAttack;

        /// <summary>
        /// Calculates attack result based on skill type and target.
        /// </summary>
        AttackResult CalculateAttackResult(IKillable target, Element element, int minAttack, int maxAttack, int minMagicAttack, int maxMagicAttack, Skill skill = null);

        /// <summary>
        /// The calculation of the attack success.
        /// </summary>
        /// <param name="target">target</param>
        /// <param name="typeAttack">type of attack</param>
        /// <param name="skill">skill if any</param>
        /// <returns>true if attack hits target, otherwise false</returns>
        bool AttackSuccessRate(IKillable target, TypeAttack typeAttack, Skill skill = null);

        /// <summary>
        /// Calculates element multiplier based on attack and defence elements.
        /// </summary>
        double GetElementFactor(Element attackElement, Element defenceElement);

        /// <summary>
        /// Attack range gained from the weapon.
        /// </summary>
        ushort WeaponAttackRange { get; set; }

        /// <summary>
        /// Additional attack range gained from buffs.
        /// </summary>
        ushort ExtraAttackRange { get; set; }

        int NextAttackTime { get; }
        DateTime LastTimeAutoAttack { get; set; }
        DateTime SkipAutoAttackRequestTime { get; set; }
        bool SkipNextAutoAttack { get; set; }
    }
}
