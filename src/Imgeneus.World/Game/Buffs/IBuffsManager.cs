using Imgeneus.World.Game.Player;
using MvvmHelpers;
using System;

namespace Imgeneus.World.Game.Buffs
{
    public interface IBuffsManager
    {
        /// <summary>
        /// Inits buffs from database.
        /// </summary>
        void Init(int ownerId);

        #region Active buffs

        /// <summary>
        /// Active buffs, that increase character characteristic, attack, defense etc.
        /// Don't update it directly, use <see cref="AddActiveBuff"/>.
        /// </summary>
        ObservableRangeCollection<Buff> ActiveBuffs { get; }

        /// <summary>
        /// Event, that is fired, when buff is added.
        /// </summary>
        public event Action<int, Buff> OnBuffAdded;

        /// <summary>
        /// Event, that is fired, when buff is removed.
        /// </summary>
        public event Action<int, Buff> OnBuffRemoved;


        /// <summary>
        /// Skill or buff that is called peridically, e.g. periodical healing.
        /// </summary>
        public event Action<int, Buff, AttackResult> OnSkillKeep;

        /// <summary>
        /// Updates collection of active buffs.
        /// </summary>
        /// <param name="skill">skill, that client sends</param>
        /// <param name="creator">buff creator</param>
        /// <returns>Newly added or updated active buff</returns>
        public Buff AddActiveBuff(Skill skill, IKiller creator);

        #endregion

        #region Passive buffs

        /// <summary>
        /// Passive buffs, that increase character characteristic, attack, defense etc.
        /// Don't update it directly, use instead "AddPassiveBuff".
        /// </summary>
        ObservableRangeCollection<Buff> PassiveBuffs { get; }

        #endregion
    }
}
