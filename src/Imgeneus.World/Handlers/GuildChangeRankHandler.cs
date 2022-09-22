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
    public class GuildChangeRankHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildChangeRankHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_USER_STATE)]
        public async Task Handle(WorldClient client, GuildUserStatePacket packet)
        {
            if (!_guildManager.HasGuild || _guildManager.GuildMemberRank > 3)
                return;

            var rank = await _guildManager.TryChangeRank(packet.CharacterId, packet.Demote);
            if (rank == 0)
                return;

            if (_gameWorld.Players.ContainsKey(packet.CharacterId))
                _gameWorld.Players[packet.CharacterId].GuildManager.GuildMemberRank = rank;

            foreach (var member in _guildManager.GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                _packetFactory.SendGuildUserChangeRank(guildPlayer.GameSession.Client, packet.CharacterId, rank);
            }
        }
    }
}
