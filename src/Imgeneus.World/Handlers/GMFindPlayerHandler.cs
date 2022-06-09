using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMFindPlayerHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMFindPlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_FIND_PLAYER)]
        public void Handle(WorldClient client, GMFindPlayerPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
                _packetFactory.SendGmCommandError(client, PacketType.GM_FIND_PLAYER);
            else
            {
                _packetFactory.SendGmCommandSuccess(client);
                _packetFactory.SendCharacterPosition(client, player);
            }
        }
    }
}
