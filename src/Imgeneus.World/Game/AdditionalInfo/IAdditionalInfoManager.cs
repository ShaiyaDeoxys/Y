using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Stats;

namespace Imgeneus.World.Game.AdditionalInfo
{
    public interface IAdditionalInfoManager
    {
        void Init(int ownerId, Race race, CharacterProfession profession, byte hair, byte face, byte height, Gender gender);

        Race Race { get; set; }
        CharacterProfession Class { get; set; }
        byte Hair { get; set; }
        byte Face { get; set; }
        byte Height { get; set; }
        Gender Gender { get; set; }

        /// <summary>
        /// Gets the character's primary stat
        /// </summary>
        CharacterStatEnum GetPrimaryStat();
    }
}
