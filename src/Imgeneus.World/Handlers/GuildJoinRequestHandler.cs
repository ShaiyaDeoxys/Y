using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GuildJoinRequestHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildJoinRequestHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_JOIN_REQUEST)]
        public async Task Handle(WorldClient client, GuildJoinRequestPacket packet)
        {
            if (_guildManager.HasGuild || _gameSession.Character.Id == 0)
            {
                _packetFactory.SendGuildJoinRequest(client, false);
                return;
            }

            var success = await _guildManager.RequestJoin(packet.GuildId, _gameWorld.Players[_gameSession.Character.Id]);
            _packetFactory.SendGuildJoinRequest(client, success);
        }
    }
}
