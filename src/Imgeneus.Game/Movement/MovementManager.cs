using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Movement
{
    public class MovementManager : IMovementManager
    {
        private readonly ILogger<MovementManager> _logger;

        private uint _ownerId;

        public MovementManager(ILogger<MovementManager> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("MovementManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~MovementManager()
        {
            _logger.LogDebug("MovementManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, float x, float y, float z, ushort angle, MoveMotion motion)
        {
            _ownerId = ownerId;

            PosX = x;
            PosY = y;
            PosZ = z;
            Angle = angle;
            MoveMotion = motion;
        }

        #endregion

        public float PosX { get; set; }

        public float PosY { get; set; }

        public float PosZ { get; set; }

        public ushort Angle { get; set; }

        public MoveMotion MoveMotion { get; set; }

        public event Action<uint, float, float, float, ushort, MoveMotion> OnMove;

        public void RaisePositionChanged()
        {
            OnMove?.Invoke(_ownerId, PosX, PosY, PosZ, Angle, MoveMotion);
        }
    }
}
