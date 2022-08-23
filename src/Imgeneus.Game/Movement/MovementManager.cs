using Imgeneus.Database.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

        public Task Clear()
        {
            Motion = Motion.None;
            return Task.CompletedTask;
        }


        #endregion

        #region Move

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

        #endregion


        #region Motion
        public event Action<uint, Motion> OnMotion;

        private Motion _motion;
        public Motion Motion
        {
            get => _motion;
            set
            {
                if (value == Motion.None || value == Motion.Sit)
                {
                    _motion = value;
                }

                OnMotion?.Invoke(_ownerId, value);
            }
        }

        #endregion
    }
}
