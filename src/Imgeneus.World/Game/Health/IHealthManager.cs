using Imgeneus.Database.Entities;
using System;

namespace Imgeneus.World.Game.Health
{
    /// <summary>
    /// Manages HP, MP, SP.
    /// </summary>
    public interface IHealthManager
    {
        public void Init(int ownerId, int currentHP, int currentSP, int currentMP, int? constHP = null, int? constSP = null, int? constMP = null, CharacterProfession? profession = null);

        /// <summary>
        /// Max health.
        /// </summary>
        public int MaxHP { get; }

        /// <summary>
        /// Max stamina.
        /// </summary>
        public int MaxSP { get; }

        /// <summary>
        /// Max mana.
        /// </summary>
        public int MaxMP { get; }

        /// <summary>
        /// Event, that is fired, when max hp changes.
        /// </summary>
        public event Action<int, int> OnMaxHPChanged;

        /// <summary>
        /// Event, that is fired, when max sp changes.
        /// </summary>
        public event Action<int, int> OnMaxSPChanged;

        /// <summary>
        /// Event, that is fired, when max mp changes.
        /// </summary>
        public event Action<int, int> OnMaxMPChanged;

        /// <summary>
        /// Only for players.
        /// </summary>
        public CharacterProfession? Class { get; }

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
        public int CurrentHP { get; }

        /// <summary>
        /// Current stamina.
        /// </summary>
        public int CurrentSP { get; set; }

        /// <summary>
        /// Current mana.
        /// </summary>
        public int CurrentMP { get; set; }

        /// <summary>
        /// Decreases health and calculates how much damage was done in order to get who was killer later on.
        /// </summary>
        /// <param name="hp">damage hp</param>
        /// <param name="damageMaker">who has made damage</param>
        public void DecreaseHP(int hp, IKiller damageMaker);

        /// <summary>
        /// Heals target hp.
        /// </summary>
        /// <param name="hp">hp healed</param>
        public void IncreaseHP(int hp);
    }
}
