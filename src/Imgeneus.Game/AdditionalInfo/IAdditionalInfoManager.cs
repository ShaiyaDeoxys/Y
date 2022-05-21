using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Stats;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.AdditionalInfo
{
    public interface IAdditionalInfoManager : ISessionedService
    {
        void Init(int ownerId, Race race, CharacterProfession profession, byte hair, byte face, byte height, Gender gender, Mode grow, uint points);

        Race Race { get; set; }
        CharacterProfession Class { get; set; }
        byte Hair { get; }
        byte Face { get; }
        byte Height { get; }
        Gender Gender { get; }

        /// <summary>
        /// Beginner, Normal, Hard or Ultimate.
        /// </summary>
        Mode Grow { get; set; }

        /// <summary>
        /// Account points, used for item mall or online shop purchases.
        /// </summary>
        uint Points { get; set; }

        /// <summary>
        /// Gets the character's primary stat
        /// </summary>
        CharacterStatEnum GetPrimaryStat();

        /// <summary>
        /// Event, that is fired, when player changes appearance.
        /// </summary>
        event Action<int, byte, byte, byte, byte> OnAppearanceChanged;

        /// <summary>
        /// Changes player's appearance.
        /// </summary>
        /// <param name="hair">new hair</param>
        /// <param name="face">new face</param>
        /// <param name="size">new size</param>
        /// <param name="sex">new sex</param>
        Task ChangeAppearance(byte hair, byte face, byte size, byte sex);
    }
}
