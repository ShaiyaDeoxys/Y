using System;

namespace Imgeneus.Game.Blessing
{
    public interface IBlessManager
    {
        /// <summary>
        /// Bless amount of light fraction.
        /// </summary>
        public int LightAmount { get; set; }

        /// <summary>
        /// Bless amount of dark fraction.
        /// </summary>
        public int DarkAmount { get; set; }

        /// <summary>
        /// The event, that is fired, when the amount of light bless changes.
        /// </summary>
        event Action<BlessArgs> OnLightBlessChanged;

        /// <summary>
        /// The event, that is fired, when the amount of dark bless changes.
        /// </summary>
        event Action<BlessArgs> OnDarkBlessChanged;

        /// <summary>
        /// Remaining time till full bless ends.
        /// </summary>
        uint RemainingTime { get; }

        /// <summary>
        /// Indicates if now full bless is running.
        /// </summary>
        bool IsFullBless { get; }

        /// <summary>
        /// When bless amount >= <see cref="SP_MP_SIT"/> it recovers more sp, mp during break.
        /// </summary>
        public const int SP_MP_SIT = 150;

        /// <summary>
        /// When bless amount >= <see cref="HP_SIT"/> it adds sp, mp during break.
        /// </summary>
        public const int HP_SIT = 300;

        /// <summary>
        /// When bless amount >= <see cref="LINK_EXTRACT_LAPIS"/> it adds several % to link and extract lapis.
        /// </summary>
        public const int LINK_EXTRACT_LAPIS = 1200;

        /// <summary>
        /// When bless amount >= <see cref="CAST_TIME_DISPOSABLE_ITEMS"/> it reduces cast time of disposable items.
        /// </summary>
        public const int CAST_TIME_DISPOSABLE_ITEMS = 1350;

        /// <summary>
        ///  When bless amount >= <see cref="EXP_LOSS"/> it reduces exp loss, when player died.
        /// </summary>
        public const int EXP_LOSS = 1500;

        /// <summary>
        /// When bless amount >= <see cref="SHOOTING_MAGIC_DEFENCE"/> it increases shooting/magic defence power.
        /// </summary>
        public const int SHOOTING_MAGIC_DEFENCE = 2100;

        /// <summary>
        /// When bless amount >= <see cref="PHYSICAL_DEFENCE"/> it increases physical defence power.
        /// </summary>
        public const int PHYSICAL_DEFENCE = 2250;

        /// <summary>
        /// When bless amount >= <see cref="REPAIR_COST"/> it reduces repair costs.
        /// </summary>
        public const int REPAIR_COST = 2700;

        /// <summary>
        /// When bless amount >= <see cref="SP_MP_SIT"/> it recovers more hp, sp, mp during battle.
        /// </summary>
        public const int HP_SP_MP_BATTLE = 8400;

        /// <summary>
        /// When bless amount >= <see cref="MAX_HP_SP_MP"/> it increases max hp, mp, sp.
        /// </summary>
        public const int MAX_HP_SP_MP = 10200;

        /// <summary>
        /// When bless amount >= <see cref="STATS"/> it increases stats.
        /// </summary>
        public const int STATS = 12000;

        /// <summary>
        /// When bless amount >= <see cref="FULL_BLESS_BONUS"/> it increases critical hit rate, evasion of all attacks (shooting/magic/physical).
        /// </summary>
        public const int FULL_BLESS_BONUS = 12288;
    }
}
