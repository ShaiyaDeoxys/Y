using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class EnteredMapHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public EnteredMapHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.CHARACTER_ENTERED_MAP)]
        public void Handle(WorldClient client, CharacterEnteredMapPacket packet)
        {
            _gameWorld.LoadPlayerInMap(_gameSession.CharId);

            // Send map values.
            //SendWeather();
            //SendObelisks();
            _packetFactory.SendCharacterShape(client, _gameWorld.Players[_gameSession.CharId]); // Should fix the issue with dye color, when first connection.
        }
    }
}
