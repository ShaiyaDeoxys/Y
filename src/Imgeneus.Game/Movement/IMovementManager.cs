using Imgeneus.Database.Constants;
using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Movement
{
    public interface IMovementManager : ISessionedService
    {
        void Init(uint ownerId, float x, float y, float z, ushort angle, MoveMotion moveMotion);

        /// <summary>
        /// X coordinate.
        /// </summary>
        float PosX { get; set; }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        float PosY { get; set; }

        /// <summary>
        /// Z coordinate.
        /// </summary>
        float PosZ { get; set; }

        /// <summary>
        /// Angle.
        /// </summary>
        ushort Angle { get; set; }

        /// <summary>
        ///  Set to 1 if you want character running or to 0 if character is "walking", same for mobs.
        ///  Used to change with Tab in previous episodes.
        /// </summary>
        MoveMotion MoveMotion { get; set; }

        /// <summary>
        /// Event, that is fired, when owner changes his position.
        /// </summary>
        event Action<uint, float, float, float, ushort, MoveMotion> OnMove;

        /// <summary>
        /// Raises event <see cref="OnMove"/>.
        /// </summary>
        void RaisePositionChanged();

        /// <summary>
        /// Event, that is fires, when character makes any motion.
        /// </summary>
        event Action<uint, Motion> OnMotion;


        /// <summary>
        /// Motion, like sit.
        /// </summary>
        Motion Motion { get; set; }
    }
}
