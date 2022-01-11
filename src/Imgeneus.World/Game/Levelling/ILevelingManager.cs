using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Levelling
{
    public interface ILevelingManager
    {
        public void Init(ushort level, CharacterProfession? profession = null, int? constHP = null, int? constSP = null, int? constMP = null);

        /// <summary>
        /// Current level.
        /// </summary>
        public ushort Level { get; set; }

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
    }
}
