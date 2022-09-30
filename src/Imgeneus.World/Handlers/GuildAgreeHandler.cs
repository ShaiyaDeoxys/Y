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
    public class GuildAgreeHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildAgreeHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_CREATE_AGREE)]
        public async Task Handle(WorldClient client, GuildAgreePacket packet)
        {
            if (_guildManager.CreationRequest is null || !_guildManager.CreationRequest.Acceptance.ContainsKey(_gameSession.Character.Id))
                return;

            _guildManager.CreationRequest.Acceptance[_gameSession.Character.Id] = packet.Ok;

            var request = _guildManager.CreationRequest;

            if (!packet.Ok)
            {
                foreach (var m in request.Members)
                {
                    _packetFactory.SendGuildCreateFailed(m.GameSession.Client, GuildCreateFailedReason.PartyMemberRejected);
                    m.GuildManager.CreationRequest = null;
                }

                request.Dispose();
                return;
            }

            var allAgree = _guildManager.CreationRequest.Acceptance.Values.All(x => x == true);
            if (!allAgree)
                return;

            var guild = await _guildManager.TryCreateGuild(_guildManager.CreationRequest.Name, _guildManager.CreationRequest.Message, _guildManager.CreationRequest.GuildCreatorId);
            if (guild is null) // Creation failed.
            {
                foreach (var m in request.Members)
                {
                    _packetFactory.SendGuildCreateFailed(m.GameSession.Client, GuildCreateFailedReason.Unknown);
                    m.GuildManager.CreationRequest = null;
                }
                request.Dispose();
                return;
            }

            foreach (var m in request.Members)
            {
                byte rank = 9;
                if (m.Id == _guildManager.CreationRequest.GuildCreatorId)
                    rank = 1;

                m.GuildManager.SetGuildInfo(guild.Id, guild.Name, rank);
                m.WarehouseManager.GuildId = guild.Id;

                await m.GuildManager.TryAddMember(m.Id, rank);
            }
            guild.Members = await _guildManager.GetMemebers(guild.Id);

            foreach (var m in request.Members)
            {
                m.GuildManager.GuildMembers.AddRange(guild.Members);
                _packetFactory.SendGuildCreateSuccess(m.GameSession.Client, guild.Id, m.GuildManager.GuildMemberRank, guild.Name, guild.Message);
                _packetFactory.SendGuildMembersOnline(m.GameSession.Client, m.GuildManager.GuildMembers, true);

                m.GuildManager.CreationRequest = null;
            }
            request.Dispose();

            foreach (var player in _gameWorld.Players.Values.ToList())
                _packetFactory.SendGuildListAdd(player.GameSession.Client, guild);
        }
    }
}
