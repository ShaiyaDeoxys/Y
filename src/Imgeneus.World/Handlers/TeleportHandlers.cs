using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class TeleportHandlers : BaseHandler
    {
        private readonly ILogger<TeleportHandlers> _logger;
        private readonly ILogger<Npc> _npcLogger;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IGuildManager _guildManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IMapsLoader _mapLoader;
        private readonly IMoveTownsConfiguration _moveTownsConfiguration;
        private readonly ICountryProvider _countryProvider;

        public TeleportHandlers(ILogger<TeleportHandlers> logger, ILogger<Npc> npcLogger, IGamePacketFactory packetFactory, IGameSession gameSession, ITeleportationManager teleportationManager, IMapProvider mapProvider, IGameWorld gameWorld, IGuildManager guildManager, IInventoryManager inventoryManager, IMapsLoader mapLoader, IMoveTownsConfiguration moveTownsConfiguration, ICountryProvider countryProvider) : base(packetFactory, gameSession)
        {
            _logger = logger;
            _npcLogger = npcLogger;
            _teleportationManager = teleportationManager;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _guildManager = guildManager;
            _inventoryManager = inventoryManager;
            _mapLoader = mapLoader;
            _moveTownsConfiguration = moveTownsConfiguration;
            _countryProvider = countryProvider;
        }

        [HandlerAction(PacketType.CHARACTER_ENTERED_PORTAL)]
        public void HandlePortalTeleport(WorldClient client, CharacterEnteredPortalPacket packet)
        {
            var success = _teleportationManager.TryTeleport(packet.PortalId, out var reason);

            if (!success)
                _packetFactory.SendPortalTeleportNotAllowed(client, reason);
        }

        [HandlerAction(PacketType.CHARACTER_TELEPORT_VIA_NPC)]
        public void HandleNpcTeleport(WorldClient client, CharacterTeleportViaNpcPacket packet)
        {
            var npc = _mapProvider.Map.GetNPC(_gameWorld.Players[_gameSession.Character.Id].CellId, packet.NpcId);
            if (npc is null)
            {
                _logger.LogWarning("Character {Id} is trying to get non-existing npc via teleport packet.", _gameSession.Character.Id);
                return;
            }

            if (!npc.ContainsGate(packet.GateId))
            {
                _logger.LogWarning("NPC type {type} type id {typeId} doesn't contain teleport gate {gateId}. Check it out!", npc.Type, npc.TypeId, packet.GateId);
                return;
            }

            if (_mapProvider.Map is GuildHouseMap)
            {
                if (!_guildManager.HasGuild)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowRank, 30);
                    return;
                }

                var allowed = _guildManager.CanUseNpc(npc.Type, npc.TypeId, out var requiredRank);
                if (!allowed)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowRank, requiredRank);
                    return;
                }

                allowed = _guildManager.HasNpcLevel(npc.Type, npc.TypeId);
                if (!allowed)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowLevel, 0);
                    return;
                }
            }

            var gate = npc.Gates[packet.GateId];

            if (_inventoryManager.Gold < gate.Cost)
            {
                _packetFactory.SendTeleportViaNpc(client, NpcTeleportNotAllowedReason.NotEnoughMoney, _inventoryManager.Gold);
                return;
            }

            var mapConfig = _mapLoader.LoadMapConfiguration((ushort)gate.MapId);
            if (mapConfig is null)
            {
                _packetFactory.SendTeleportViaNpc(client, NpcTeleportNotAllowedReason.MapCapacityIsFull, _inventoryManager.Gold);
                return;
            }

            // TODO: there should be somewhere player's level check. But I can not find it in gate config.

            _inventoryManager.Gold = (uint)(_inventoryManager.Gold - gate.Cost);
            _packetFactory.SendTeleportViaNpc(client, NpcTeleportNotAllowedReason.Success, _inventoryManager.Gold);
            _teleportationManager.Teleport((ushort)gate.MapId, gate.Position.X, gate.Position.Y, gate.Position.Z);
        }

        [HandlerAction(PacketType.TELEPORT_PRELOADED_TOWN)]
        public async Task HandleTeleportPreloadedTown(WorldClient client, TeleportPreloadedTownPacket packet)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var item))
                return;

            if (item.Special != SpecialEffect.TownTeleport)
                return;

            item.TradeQuantity = packet.GateId;
            await _inventoryManager.TryUseItem(packet.Bag, packet.Slot);
        }

        [HandlerAction(PacketType.TELEPORT_PRELOADED_AREA)]
        public void HandleTeleportPreloadedArea(WorldClient client, TeleportPreloadedAreaPacket packet)
        {
            if (!_inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var item))
            {
                _packetFactory.SendTeleportPreloadedArea(client, false);
                return;
            }

            if (!_moveTownsConfiguration.MoveTowns.TryGetValue(packet.MoveTownInfoId, out var townConfig))
            {
                _packetFactory.SendTeleportPreloadedArea(client, false);
                return;
            }

            if (_countryProvider.Country != townConfig.Country)
            {
                _packetFactory.SendTeleportPreloadedArea(client, false);
                return;
            }

            _teleportationManager.StartCastingTeleport(townConfig.MapId, townConfig.X, townConfig.Y, townConfig.Z, item);
            _packetFactory.SendTeleportPreloadedArea(client, true);
        }

        [HandlerAction(PacketType.TELEPORT_SAVE_POSITION)]
        public void HandleTeleportSavePosition(WorldClient client, TeleportSavePositionPacket packet)
        {
            var ok = _teleportationManager.TrySavePosition(packet.Index, packet.MapId, packet.X, packet.Y, packet.Z);
            _packetFactory.SendTeleportSavedPosition(client, ok, packet.Index, packet.MapId, packet.X, packet.Y, packet.Z);
        }


        [HandlerAction(PacketType.TOWN_TELEPORT)]
        public void HandleTownTeleport(WorldClient client, EmptyPacket packet)
        {
            _teleportationManager.StartTownTeleport();
        }

        [HandlerAction(PacketType.TELEPORT_TO_BATTLEGROUND)]
        public void HandleTeleportToBattleground(WorldClient client, TeleportToBattlegroundPacket packet)
        {
            if (!_gameWorld.AvailableMapIds.Contains(packet.MapId))
                return;

            if (_gameSession.Character is null)
                return;

            if (!_gameWorld.CanTeleport(_gameSession.Character, packet.MapId, out _))
                return;

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

            _teleportationManager.Teleport(packet.MapId, x, y, z);
            _packetFactory.SendTeleportToBattleground(client, true);
        }
    }
}
