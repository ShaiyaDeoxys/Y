using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.Portals;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

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
        private readonly IBlessManager _blessManager;
        private uint _ownerId;

        public TeleportationManager(ILogger<TeleportationManager> logger, IMovementManager movementManager, IMapProvider mapProvider, IDatabase database, ICountryProvider countryProvider, ILevelProvider levelProvider, IGameWorld gameWorld, IHealthManager healthManager, IBlessManager blessManager)
        {
            _logger = logger;
            _movementManager = movementManager;
            _mapProvider = mapProvider;
            _database = database;
            _countryProvider = countryProvider;
            _levelProvider = levelProvider;
            _gameWorld = gameWorld;
            _healthManager = healthManager;
            _blessManager = blessManager;
            _castingTimer.Elapsed += OnCastingTimer_Elapsed;
            _townTimer.Elapsed += OnTownTimer_Elapsed;
            _healthManager.OnGotDamage += HealthManager_OnGotDamage;
            _movementManager.OnMove += MovementManager_OnMove;
#if DEBUG
            _logger.LogDebug("TeleportationManager {hashcode} created", GetHashCode());
#endif

            SavedPositions = new ReadOnlyDictionary<byte, (ushort MapId, float X, float Y, float Z)>(_savedPositions);
        }

#if DEBUG
        ~TeleportationManager()
        {
            _logger.LogDebug("TeleportationManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, IEnumerable<DbCharacterSavePositions> savedPositions)
        {
            _ownerId = ownerId;

            foreach (var pos in savedPositions)
                _savedPositions.TryAdd(pos.Slot, (pos.MapId, pos.X, pos.Y, pos.Z));

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

            var savedPositions = await _database.CharacterSavePositions.Where(x => x.CharacterId == _ownerId).ToListAsync();
            _database.CharacterSavePositions.RemoveRange(savedPositions);

            foreach (var pos in _savedPositions)
            {
                _database.CharacterSavePositions.Add(
                    new DbCharacterSavePositions()
                    {
                        CharacterId = _ownerId,
                        Slot = pos.Key,
                        MapId = pos.Value.MapId,
                        X = pos.Value.X,
                        Y = pos.Value.Y,
                        Z = pos.Value.Z
                    });
            }

            await _database.SaveChangesAsync();
        }

        public void Dispose()
        {
            _castingTimer.Elapsed -= OnCastingTimer_Elapsed;
            _townTimer.Elapsed -= OnTownTimer_Elapsed;
            _healthManager.OnGotDamage -= HealthManager_OnGotDamage;
            _movementManager.OnMove -= MovementManager_OnMove;
        }

        #endregion

        #region Teleport

        public event Action<uint, ushort, float, float, float, bool, bool> OnTeleporting;

        /// <summary>
        /// Indicator if character is teleporting between maps.
        /// </summary>
        public bool IsTeleporting { get; set; }

        public void Teleport(ushort mapId, float x, float y, float z, bool teleportedByAdmin = false, bool summonedByAdmin = false)
        {
            IsTeleporting = true;

            if (_gameWorld.CanTeleport(_gameWorld.Players[_ownerId], mapId, out var reason, teleportedByAdmin || summonedByAdmin))
                _mapProvider.NextMapId = mapId;
            else
                if (reason == PortalTeleportNotAllowedReason.Unknown)
                _mapProvider.NextMapId = 0;
            else if (teleportedByAdmin)
                _mapProvider.NextMapId = mapId;

            _movementManager.PosX = x;
            _movementManager.PosY = y;
            _movementManager.PosZ = z;

            OnTeleporting?.Invoke(_ownerId, _mapProvider.NextMapId, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ, teleportedByAdmin, summonedByAdmin);

            var prevMapId = _mapProvider.Map.Id;
            if (prevMapId == mapId)
            {
                IsTeleporting = false;
                _movementManager.RaisePositionChanged(); // Raise coordinate change for cell update.
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

            if (!portal.IsOpen)
            {
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

        #endregion

        #region Casting teleport

        public event Action<uint> OnCastingTeleport;

        public event Action OnCastingTeleportFinished;

        public (ushort MapId, float X, float Y, float Z) CastingPosition { get; private set; }

        private Timer _castingTimer = new Timer() { AutoReset = false };

        public Item CastingItem { get; private set; }

        public void StartCastingTeleport(ushort mapId, float x, float y, float z, Item item, bool skeepTimer = false)
        {
            OnCastingTeleport?.Invoke(_ownerId);

            CastingPosition = (mapId, x, y, z);
            CastingItem = item;

            _castingTimer.Interval = 5000;

            if (_countryProvider.Country == CountryType.Light && _blessManager.LightAmount >= IBlessManager.CAST_TIME_DISPOSABLE_ITEMS)
                _castingTimer.Interval = 4500;
            if (_countryProvider.Country == CountryType.Dark && _blessManager.DarkAmount >= IBlessManager.CAST_TIME_DISPOSABLE_ITEMS)
                _castingTimer.Interval = 4500;

            _castingTimer.Start();
        }

        private void OnCastingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (CastingPosition == (0, 0, 0, 0) || CastingItem == null)
                return;

            OnCastingTeleportFinished?.Invoke();
            Teleport(CastingPosition.MapId, CastingPosition.X, CastingPosition.Y, CastingPosition.Z);
            CastingItem = null;
            CastingPosition = (0, 0, 0, 0);
        }

        private void HealthManager_OnGotDamage(uint sender, IKiller damageMaker, int damage)
        {
            CancelCasting();
            CancelTownTeleport();
        }

        private void MovementManager_OnMove(uint senderId, float x, float y, float z, ushort angle, MoveMotion motion)
        {
            CancelCasting();
            CancelTownTeleport();
        }

        private void CancelCasting()
        {
            CastingPosition = (0, 0, 0, 0);
            CastingItem = null;
            _castingTimer.Stop();
        }

        #endregion

        #region Save position

        public byte MaxSavedPoints { get; set; } = 1;

        private ConcurrentDictionary<byte, (ushort MapId, float X, float Y, float Z)> _savedPositions { get; init; } = new();
        public IReadOnlyDictionary<byte, (ushort MapId, float X, float Y, float Z)> SavedPositions { get; init; }

        public bool TrySavePosition(byte index, ushort mapId, float x, float y, float z)
        {
            if (index > 4 || index > MaxSavedPoints || index < 1)
                return false;

            if (_savedPositions.ContainsKey(index))
                _savedPositions.TryRemove(index, out var _);

            return _savedPositions.TryAdd(index, (mapId, x, y, z));
        }

        #endregion

        #region Town teleport 

        private Timer _townTimer = new Timer() { AutoReset = false, Interval = 10000 };

        public void StartTownTeleport()
        {
            _townTimer.Start();
        }

        private void OnTownTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_gameWorld.Players.TryGetValue(_ownerId, out var player))
            {
                var spawn = _mapProvider.Map.GetRebirthMap(player);
                Teleport(spawn.MapId, spawn.X, spawn.Y, spawn.Z);
            }
        }

        private void CancelTownTeleport()
        {
            _townTimer.Stop();
        }

        #endregion
    }
}
