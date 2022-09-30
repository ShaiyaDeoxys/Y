using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GuildJoinResultHandler : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IGameWorld _gameWorld;

        public GuildJoinResultHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.GUILD_JOIN_RESULT_USER)]
        public async Task Handle(WorldClient client, GuildJoinResultPacket packet)
        {
            if (!_guildManager.HasGuild || _guildManager.GuildMemberRank > 3)
                return;

            await _guildManager.RemoveRequestJoin(packet.CharacterId);

            _gameWorld.Players.TryGetValue(packet.CharacterId, out var onlinePlayer);
            if (!packet.Ok)
            {
                if (onlinePlayer != null)
                    _packetFactory.SendGuildJoinResult(onlinePlayer.GameSession.Client, false);

                return;
            }

            var dbCharacter = await _guildManager.TryAddMember(packet.CharacterId);
            if (dbCharacter is null)
            {
                if (onlinePlayer != null)
                    _packetFactory.SendGuildJoinResult(onlinePlayer.GameSession.Client, false);

                return;
            }

            // Update guild members.
            foreach (var member in _guildManager.GuildMembers.ToList())
            {
                if (!_gameWorld.Players.ContainsKey(member.Id))
                    continue;

                var guildPlayer = _gameWorld.Players[member.Id];
                guildPlayer.GuildManager.GuildMembers.Add(dbCharacter);
                _packetFactory.SendGuildUserListAdd(guildPlayer.GameSession.Client, dbCharacter, onlinePlayer != null);
            }

            // Send additional info to new member, if he is online.
            if (onlinePlayer != null)
            {
                onlinePlayer.GuildManager.SetGuildInfo(_guildManager.GuildId, _guildManager.GuildName, 9);
                onlinePlayer.GuildManager.GuildMembers.AddRange(_guildManager.GuildMembers);
                onlinePlayer.WarehouseManager.GuildId = _guildManager.GuildId;

                _packetFactory.SendGuildJoinResult(onlinePlayer.GameSession.Client, true, onlinePlayer.GuildManager.GuildId, onlinePlayer.GuildManager.GuildMemberRank, onlinePlayer.GuildManager.GuildName);
                _packetFactory.SendGuildNpcs(onlinePlayer.GameSession.Client, await onlinePlayer.GuildManager.GetGuildNpcs());

                var online = new List<DbCharacter>();
                var notOnline = new List<DbCharacter>();
                foreach (var m in _guildManager.GuildMembers)
                {
                    if (_gameWorld.Players.ContainsKey(m.Id))
                        online.Add(m);
                    else
                        notOnline.Add(m);
                }
                _packetFactory.SendGuildMembersOnline(onlinePlayer.GameSession.Client, online, true);
                _packetFactory.SendGuildMembersOnline(onlinePlayer.GameSession.Client, notOnline, false);                
            }
        }
    }
}
