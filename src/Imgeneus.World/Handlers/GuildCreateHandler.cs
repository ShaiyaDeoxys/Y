using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GuildCreateHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;

        public GuildCreateHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
        }

        [HandlerAction(PacketType.GUILD_CREATE)]
        public async Task Handle(WorldClient client, GuildCreatePacket packet)
        {
            var result = await _guildManager.CanCreateGuild(packet.Name);
            if (result != GuildCreateFailedReason.Success)
            {
                _packetFactory.SendGuildCreateFailed(client, result);
                return;
            }

            _guildManager.InitCreateRequest(packet.Name, packet.Message);

            foreach (var member in _guildManager.CreationRequest.Members)
                _packetFactory.SendGuildCreateRequest(member.GameSession.Client, _gameSession.Character.Id, packet.Name, packet.Message);
        }
    }
}
