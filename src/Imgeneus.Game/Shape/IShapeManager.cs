using System;

namespace Imgeneus.World.Game.Shape
{
    public interface IShapeManager : IDisposable
    {
        void Init(uint ownerId);

        /// <summary>
        /// Event, that is fired, when character changes shape.
        /// </summary>
        event Action<uint, ShapeEnum, uint, uint> OnShapeChange;

        /// <summary>
        /// Character shape.
        /// </summary>
        ShapeEnum Shape { get; }

        /// <summary>
        /// When Transformation buff is used, set to true.
        /// </summary>
        bool IsTranformated { get; set; }

        /// <summary>
        /// Event, that is fired, when tranform forms changes.
        /// </summary>
        event Action<uint, bool> OnTranformated;

        /// <summary>
        /// Monster shape level. Fox is 1, Wolf is 2, Knight is 3.
        /// </summary>
        ShapeEnum MonsterLevel { get; set; }

        /// <summary>
        /// Specific monster shape.
        /// </summary>
        ushort MobId { get; set; }

        /// <summary>
        /// Transformed to opposite country character.
        /// </summary>
        bool IsOppositeCountry { get; set; }

        /// <summary>
        /// Opposite country character.
        /// </summary>
        uint CharacterId { get; set; }
    }
}
