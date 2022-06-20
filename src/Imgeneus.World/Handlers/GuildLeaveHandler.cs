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
    public class GuildLeaveHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildLeaveHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_LEAVE)]
        public async Task Handle(WorldClient client, GuildLeavePacket packet)
        {
            if (!_guildManager.HasGuild)
                return;

            var ok = await _guildManager.TryRemoveMember(_gameSession.Character.Id);
            if (!ok)
            {
                _packetFactory.SendGuildMemberLeaveResult(client, false);
                return;
            }

            foreach (var member in _guildManager.GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];

                if (guildPlayer.Id == _gameSession.Character.Id)
                {
                    await guildPlayer.GuildManager.Clear();
                }
                else
                {
                    var temp = guildPlayer.GuildManager.GuildMembers.FirstOrDefault(x => x.Id == _gameSession.Character.Id);

                    if (temp != null)
                        guildPlayer.GuildManager.GuildMembers.Remove(temp);

                    _packetFactory.SendGuildMemberLeave(guildPlayer.GameSession.Client, _gameSession.Character.Id);
                }
            }

            _packetFactory.SendGuildMemberLeaveResult(client, true);
        }
    }
}
