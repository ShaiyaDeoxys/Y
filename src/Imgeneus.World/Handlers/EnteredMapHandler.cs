using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class EnteredMapHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMapProvider _mapProvider;

        public EnteredMapHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, ITeleportationManager teleportationManager, IMapProvider mapProvider) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _teleportationManager = teleportationManager;
            _mapProvider = mapProvider;
        }

        [HandlerAction(PacketType.CHARACTER_ENTERED_MAP)]
        public void Handle(WorldClient client, CharacterEnteredMapPacket packet)
        {
            _gameWorld.LoadPlayerInMap(_gameSession.CharId);
            _teleportationManager.IsTeleporting = false;

            // Send map values.
            _packetFactory.SendWeather(client, _mapProvider.Map);
            //SendObelisks();
            _packetFactory.SendCharacterShape(client, _gameWorld.Players[_gameSession.CharId]); // Should fix the issue with dye color, when first connection.
        }
    }
}
