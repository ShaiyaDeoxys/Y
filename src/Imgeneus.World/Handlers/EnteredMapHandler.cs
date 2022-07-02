using Imgeneus.Core.Structures.Configuration;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Options;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class EnteredMapHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMapProvider _mapProvider;
        private readonly IWarehouseManager _warehouseManager;
        private readonly INoticeManager _noticeManager;
        private readonly WorldConfiguration _worldConfiguration;

        public EnteredMapHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, ITeleportationManager teleportationManager, IMapProvider mapProvider, IWarehouseManager warehouseManager, IOptions<WorldConfiguration> worldConfiguration, INoticeManager noticeManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _teleportationManager = teleportationManager;
            _mapProvider = mapProvider;
            _warehouseManager = warehouseManager;
            _noticeManager = noticeManager;
            _worldConfiguration = worldConfiguration.Value;
        }

        [HandlerAction(PacketType.CHARACTER_ENTERED_MAP)]
        public async Task Handle(WorldClient client, CharacterEnteredMapPacket packet)
        {
            if (!_gameWorld.Players.ContainsKey(_gameSession.Character.Id))
            {
                _gameWorld.TryLoadPlayer(_gameSession.Character);

                if (!string.IsNullOrWhiteSpace(_worldConfiguration.WelcomeMessage))
                    _noticeManager.TrySendPlayerNotice(_worldConfiguration.WelcomeMessage, _gameSession.Character.AdditionalInfoManager.Name);
            }

            _gameWorld.LoadPlayerInMap(_gameSession.Character.Id);
            _teleportationManager.IsTeleporting = false;

            // Send map values.
            _packetFactory.SendWeather(client, _mapProvider.Map);
            _packetFactory.SendObelisks(client, _mapProvider.Map.Obelisks.Values);
            _packetFactory.SendCharacterShape(client, _gameSession.Character.Id, _gameWorld.Players[_gameSession.Character.Id]); // Should fix the issue with dye color, when first connection.

            if (_mapProvider.Map is GuildHouseMap)
                _packetFactory.SendGuildWarehouseItems(client, await _warehouseManager.GetGuildItems());
        }
    }
}
