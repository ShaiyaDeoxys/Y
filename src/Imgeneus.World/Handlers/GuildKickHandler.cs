using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GuildKickHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildKickHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_KICK)]
        public async Task Handle(WorldClient client, GuildKickPacket packet)
        {
            if (!_guildManager.HasGuild || _guildManager.GuildMemberRank > 3)
            {
                _packetFactory.SendGuildKickMember(client, false, packet.CharacterId);
                return;
            }

            var removedId = packet.CharacterId;
            var ok = await _guildManager.TryRemoveMember(removedId);
            if (!ok)
            {
                _packetFactory.SendGuildKickMember(client, false, removedId);
                return;
            }

            // Update guild members.
            foreach (var member in _guildManager.GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];

                if (guildPlayer.Id == removedId)
                    await guildPlayer.GuildManager.Clear();
                else
                {
                    var temp = guildPlayer.GuildManager.GuildMembers.FirstOrDefault(x => x.Id == removedId);

                    if (temp != null)
                        guildPlayer.GuildManager.GuildMembers.Remove(temp);
                }

                _packetFactory.SendGuildKickMember(guildPlayer.GameSession.Client, true, removedId);
                _packetFactory.SendGuildMemberRemove(guildPlayer.GameSession.Client, removedId);
            }
        }
    }
}
