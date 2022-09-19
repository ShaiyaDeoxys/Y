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
    public class GMMuteHandlers : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public GMMuteHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GM_MUTE_PLAYER)]
        public void HandleMute(WorldClient client, GMMutePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_MUTE_PLAYER);
                return;
            }

            player.ChatManager.IsMuted = true;
            _packetFactory.SendGmMutedChat(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }

        [HandlerAction(PacketType.GM_UNMUTE_PLAYER)]
        public void HandleUnmute(WorldClient client, GMUnmutePacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            var player = _gameWorld.Players.Values.FirstOrDefault(p => p.AdditionalInfoManager.Name == packet.Name);
            if (player is null)
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_MUTE_PLAYER);
                return;
            }

            player.ChatManager.IsMuted = false;
            _packetFactory.SendGmUnmutedChat(player.GameSession.Client);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
