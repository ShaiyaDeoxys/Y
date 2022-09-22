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
    public class GMStopPlayerHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMStopPlayerHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_STOP_ON)]
        public void HandleStopOn(WorldClient client, GMStopOnPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_STOP_ON);
                return;
            }

            player.SpeedManager.Immobilize = true;

            _packetFactory.SendGmStopOn(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_STOP_OFF)]
        public void HandleStopOff(WorldClient client, GMStopOffPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_STOP_OFF);
                return;
            }

            player.SpeedManager.Immobilize = false;

            _packetFactory.SendGmStopOff(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
