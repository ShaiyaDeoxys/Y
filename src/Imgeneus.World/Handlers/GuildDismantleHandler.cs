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
    public class GuildDismantleHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildDismantleHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_DISMANTLE)]
        public async Task Handle(WorldClient client, GuildDismantlePacket packet)
        {
            if (!_guildManager.HasGuild || _guildManager.GuildMemberRank != 1)
                return;

            var ok = await _guildManager.TryDeleteGuild();
            if (!ok)
                return;

            foreach (var player in _gameWorld.Players.Values.ToList())
                _packetFactory.SendGuildListRemove(player.GameSession.Client, _guildManager.GuildId);

            foreach (var member in _guildManager.GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                await guildPlayer.GuildManager.Clear();
                _packetFactory.SendGuildDismantle(guildPlayer.GameSession.Client);
            }
        }
    }
}
