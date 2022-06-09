using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class CharacterShapeHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public CharacterShapeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.CHARACTER_SHAPE)]
        public void Handle(WorldClient client, CharacterShapePacket packet)
        {
            var character = _gameWorld.Players[packet.CharacterId];
            if (character is null)
                return;

            _packetFactory.SendCharacterShape(client, character.Id, character);
        }
    }
}
