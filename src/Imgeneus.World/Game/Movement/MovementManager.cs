using Imgeneus.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Movement
{
    public class MovementManager : IMovementManager
    {
        private readonly ILogger<MovementManager> _logger;
        private readonly IDatabase _database;

        private int _ownerId;

        public MovementManager(ILogger<MovementManager> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;
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

        public void Init(int ownerId, float x, float y, float z, ushort angle, MoveMotion motion)
        {
            _ownerId = ownerId;

            PosX = x;
            PosY = y;
            PosZ = z;
            Angle = angle;
            MoveMotion = motion;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                _logger.LogError("Character {id} is not found in database.", _ownerId);

            character.PosX = PosX;
            character.PosY = PosY;
            character.PosZ = PosZ;
            character.Angle = Angle;

            await _database.SaveChangesAsync();
        }

        #endregion

        public float PosX { get; set; }

        public float PosY { get; set; }

        public float PosZ { get; set; }

        public ushort Angle { get; set; }

        public MoveMotion MoveMotion { get; set; }

        public event Action<int, float, float, float, ushort, MoveMotion> OnMove;

        public void RaisePositionChanged()
        {
            OnMove?.Invoke(_ownerId, PosX, PosY, PosZ, Angle, MoveMotion);
        }
    }
}
