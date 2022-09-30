using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMTeleportHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IMapsLoader _mapLoader;
        private readonly ICountryProvider _countryProvider;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMovementManager _movementManager;
        private readonly IPartyManager _partyManager;
        private readonly IGuildManager _guildManager;

        public GMTeleportHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IMapsLoader mapLoader, ICountryProvider countryProvider, ITeleportationManager teleportationManager, IMovementManager movementManager, IPartyManager partyManager, IGuildManager guildManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _mapLoader = mapLoader;
            _countryProvider = countryProvider;
            _teleportationManager = teleportationManager;
            _movementManager = movementManager;
            _partyManager = partyManager;
            _guildManager = guildManager;
        }

        [HandlerAction(PacketType.GM_TELEPORT_MAP)]
        public void HandleTeleportMap(WorldClient client, GMTeleportMapPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_gameWorld.AvailableMapIds.Contains(packet.MapId))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_TELEPORT_MAP);
                return;
            }

            float x = 100;
            float y = 100;
            float z = 100;
            var spawn = _mapLoader.LoadMapConfiguration(packet.MapId).Spawns.FirstOrDefault(s => ((int)s.Faction == 1 && _countryProvider.Country == CountryType.Light) || ((int)s.Faction == 2 && _countryProvider.Country == CountryType.Dark));
            if (spawn != null)
            {
                x = spawn.Area.LowerLimit.X;
                y = spawn.Area.LowerLimit.Y;
                z = spawn.Area.LowerLimit.Z;
            }

            _packetFactory.SendGmCommandSuccess(client);

            _teleportationManager.Teleport(packet.MapId, x, y, z, true);
        }

        [HandlerAction(PacketType.GM_TELEPORT_MAP_COORDINATES)]
        public void HandleTeleportMapCoordinates(WorldClient client, GMTeleportMapCoordinatesPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var (newPosX, newPosZ, mapId) = packet;

            if (!_gameWorld.AvailableMapIds.Contains(mapId))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_TELEPORT_MAP_COORDINATES);
                return;
            }

            _packetFactory.SendGmCommandSuccess(client);

            _teleportationManager.Teleport(mapId, newPosX, _movementManager.PosY, newPosZ, true);
        }

        [HandlerAction(PacketType.GM_TELEPORT_TO_PLAYER)]
        public void HandleTeleportToPlayer(WorldClient client, GMTeleportToPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
                _packetFactory.SendGmCommandError(client, PacketType.GM_TELEPORT_TO_PLAYER);
            else
            {
                // Teleport to party instance map.
                if (player.Map is IPartyMap)
                {
                    _partyManager.Party = null;
                    var mapId = _gameWorld.PartyMaps.FirstOrDefault(m => m.Value == player.Map).Key;
                    _partyManager.PreviousPartyId = mapId;
                }

                if (player.Map is IGuildMap)
                    _guildManager.SetGuildInfo(player.GuildManager.GuildId, player.GuildManager.GuildName, 9);

                _teleportationManager.Teleport(player.Map.Id, player.PosX, player.PosY, player.PosZ, true);

                _packetFactory.SendGmCommandSuccess(client);
                _packetFactory.SendGmTeleportToPlayer(client, player);
            }
        }
    }
}
