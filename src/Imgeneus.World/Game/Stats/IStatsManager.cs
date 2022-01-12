using Imgeneus.Network.Packets.Game;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Stats
{
    public interface IStatsManager
    {
        /// <summary>
        /// Inits constant stats.
        /// </summary>
        void Init(int ownerId, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc, ushort statPoints);

        /// <summary>
        /// Str value, needed for attack calculation.
        /// </summary>
        int TotalStr { get; }

        /// <summary>
        /// Dex value, needed for damage calculation.
        /// </summary>
        int TotalDex { get; }

        /// <summary>
        /// Rec value, needed for HP calculation.
        /// </summary>
        int TotalRec { get; }

        /// <summary>
        /// Int value, needed for damage calculation.
        /// </summary>
        int TotalInt { get; }

        /// <summary>
        /// Wis value, needed for damage calculation.
        /// </summary>
        int TotalWis { get; }

        /// <summary>
        /// Luck value, needed for critical damage calculation.
        /// </summary>
        int TotalLuc { get; }

        /// <summary>
        /// Constant str.
        /// </summary>
        ushort Strength { get; }

        /// <summary>
        /// Constant dex.
        /// </summary>
        ushort Dexterity { get; }

        /// <summary>
        /// Constant rec.
        /// </summary>
        ushort Reaction { get; }

        /// <summary>
        /// Constant int.
        /// </summary>
        ushort Intelligence { get; }

        /// <summary>
        /// Constant luc.
        /// </summary>
        ushort Luck { get; }

        /// <summary>
        /// Constant wis.
        /// </summary>
        ushort Wisdom { get; }

        /// <summary>
        /// Yellow strength stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraStr { get; set; }

        /// <summary>
        /// Yellow dexterity stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraDex { get; set; }

        /// <summary>
        /// Yellow rec stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraRec { get; set; }

        /// <summary>
        /// Yellow intelligence stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraInt { get; set; }

        /// <summary>
        /// Yellow luck stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraLuc { get; set; }

        /// <summary>
        /// Yellow wisdom stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraWis { get; set; }

        /// <summary>
        /// Physical defense from equipment and buffs.
        /// </summary>
        int ExtraDefense { get; set; }

        /// <summary>
        /// Magical resistance from equipment and buffs.
        /// </summary>
        int ExtraResistance { get; set; }

        /// <summary>
        /// Absorbs damage regardless of REC value.
        /// </summary>
        ushort Absorption { get; set; }

        /// <summary>
        /// Free stat points, that player can set.
        /// </summary>
        ushort StatPoint { get; }

        /// <summary>
        /// Tries to set const stats.
        /// </summary>
        Task<bool> TrySetStats(ushort? str = null, ushort? dex = null, ushort? rec = null, ushort? intl = null, ushort? wis = null, ushort? luc = null, ushort? statPoints = null);
    }
}
