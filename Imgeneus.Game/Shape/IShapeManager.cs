using System;

namespace Imgeneus.World.Game.Shape
{
    public interface IShapeManager : IDisposable
    {
        void Init(int ownerId);

        /// <summary>
        /// Event, that is fired, when character changes shape.
        /// </summary>
        public event Action<int, ShapeEnum, int, int> OnShapeChange;

        /// <summary>
        /// Character shape.
        /// </summary>
        public ShapeEnum Shape { get; }
    }
}
