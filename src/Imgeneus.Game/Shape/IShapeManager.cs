using System;

namespace Imgeneus.World.Game.Shape
{
    public interface IShapeManager : IDisposable
    {
        void Init(int ownerId);

        /// <summary>
        /// Event, that is fired, when character changes shape.
        /// </summary>
        event Action<int, ShapeEnum, int, int> OnShapeChange;

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
        event Action<int, bool> OnTranformated;
    }
}
