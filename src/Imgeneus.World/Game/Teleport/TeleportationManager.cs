using Imgeneus.Database;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Portals;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Imgeneus.World.Game.Teleport
{
    public class TeleportationManager : ITeleportationManager
    {
        private readonly ILogger<TeleportationManager> _logger;
        private readonly IMovementManager _movementManager;
        private readonly IMapProvider _mapProvider;
        private readonly IDatabase _database;
        private readonly ICountryProvider _countryProvider;
        private readonly ILevelProvider _levelProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IHealthManager _healthManager;
        private int _ownerId;

        public TeleportationManager(ILogger<TeleportationManager> logger, IMovementManager movementManager, IMapProvider mapProvider, IDatabase database, ICountryProvider countryProvider, ILevelProvider levelProvider, IGameWorld gameWorld, IHealthManager healthManager)
        {
            _logger = logger;
            _movementManager = movementManager;
            _mapProvider = mapProvider;
            _database = database;
            _countryProvider = countryProvider;
            _levelProvider = levelProvider;
            _gameWorld = gameWorld;
            _healthManager = healthManager;
            _castingTimer.Elapsed += OnCastingTimer_Elapsed;
            _healthManager.OnGotDamage += CancelCasting;
#if DEBUG
            _logger.LogDebug("TeleportationManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~TeleportationManager()
        {
            _logger.LogDebug("TeleportationManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int ownerId)
        {
            _ownerId = ownerId;

            IsTeleporting = true;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                _logger.LogError("Character {id} is not found in database.", _ownerId);

            character.PosX = _movementManager.PosX;
            character.PosY = _movementManager.PosY;
            character.PosZ = _movementManager.PosZ;
            character.Angle = _movementManager.Angle;
            character.Map = _mapProvider.NextMapId;

            await _database.SaveChangesAsync();

        }

        public void Dispose()
        {
            _castingTimer.Elapsed -= OnCastingTimer_Elapsed;
            _healthManager.OnGotDamage -= CancelCasting;
        }

        #endregion

        public event Action<int, ushort, float, float, float, bool> OnTeleporting;

        /// <summary>
        /// Indicator if character is teleporting between maps.
        /// </summary>
        public bool IsTeleporting { get; set; }

        public void Teleport(ushort mapId, float x, float y, float z, bool teleportedByAdmin = false)
        {
            IsTeleporting = true;

            _mapProvider.NextMapId = mapId;

            _movementManager.PosX = x;
            _movementManager.PosY = y;
            _movementManager.PosZ = z;

            OnTeleporting?.Invoke(_ownerId, _mapProvider.NextMapId, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ, teleportedByAdmin);

            var prevMapId = _mapProvider.Map.Id;
            if (prevMapId == mapId)
            {
                IsTeleporting = false;
            }
            else
            {
                _mapProvider.Map.UnloadPlayer(_ownerId);
            }
        }

        public bool TryTeleport(byte portalIndex, out PortalTeleportNotAllowedReason reason)
        {
            reason = PortalTeleportNotAllowedReason.Unknown;
            if (_mapProvider.Map.Portals.Count <= portalIndex)
            {
                _logger.LogWarning("Unknown portal {portalIndex} for map {mapId}. Send from character {id}.", portalIndex, _mapProvider.Map.Id, _ownerId);
                return false;
            }

            var portal = _mapProvider.Map.Portals[portalIndex];
            if (!portal.IsInPortalZone(_movementManager.PosX, _movementManager.PosY, _movementManager.PosZ))
            {
                _logger.LogWarning("Character position is not in portal, map {mapId}. Portal index {portalIndex}. Send from character {id}.", _mapProvider.Map.Id, portalIndex, _ownerId);
                return false;
            }

            if (!portal.IsSameFaction(_countryProvider.Country))
            {
                return false;
            }

            if (!portal.IsRightLevel(_levelProvider.Level))
            {
                return false;
            }

            if (_gameWorld.CanTeleport(_gameWorld.Players[_ownerId], portal.MapId, out reason))
            {
                Teleport(portal.MapId, portal.Destination_X, portal.Destination_Y, portal.Destination_Z);
                return true;
            }
            else
            {
                return false;
            }
        }

        public event Action<int> OnCastingTeleport;

        private (ushort MapId, float X, float Y, float Z) CastingPosition;

        private Timer _castingTimer = new Timer() { AutoReset = false, Interval = 5000 };

        public void StartCastingTeleport(ushort mapId, float x, float y, float z, bool skeepTimer = false)
        {
            OnCastingTeleport?.Invoke(_ownerId);

            CastingPosition = (mapId, x, y, z);
            _castingTimer.Start();
        }

        private void OnCastingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CastingPosition == (0, 0, 0, 0))
                return;

            Teleport(CastingPosition.MapId, CastingPosition.X, CastingPosition.Y, CastingPosition.Z);
            CastingPosition = (0, 0, 0, 0);
        }

        private void CancelCasting(int sender, IKiller damageMaker)
        {
            CastingPosition = (0, 0, 0, 0);
            _castingTimer.Stop();
        }
    }
}
