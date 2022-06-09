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
    public class GMWarningPlayerHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMWarningPlayerHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_WARNING_PLAYER)]
        public void HandleNoticeWorld(WorldClient client, GMWarningPacket packet)
        {
            var target = _gameWorld.Players.FirstOrDefault(p => p.Value.AdditionalInfoManager.Name == packet.Name).Value;

            if (target == null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_WARNING_PLAYER);
                return;
            }

            _packetFactory.SendWarning(target.GameSession.Client, packet.Message);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
