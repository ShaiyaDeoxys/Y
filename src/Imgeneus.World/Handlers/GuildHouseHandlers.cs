using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GuildHouseHandlers : BaseHandler
    {
        private readonly IGuildManager _guildManager;
        private readonly IInventoryManager _inventoryManager;

        public GuildHouseHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IGuildManager guildManager, IInventoryManager inventoryManager) : base(packetFactory, gameSession)
        {
            _guildManager = guildManager;
            _inventoryManager = inventoryManager;
        }

        [HandlerAction(PacketType.GUILD_HOUSE_BUY)]
        public async Task HandleBuyHouse(WorldClient client, GuildHouseBuyPacket packet)
        {
            if (!_guildManager.HasGuild)
                return;

            var reason = await _guildManager.TryBuyHouse();
            _packetFactory.SendGuildHouseBuy(client, reason, _inventoryManager.Gold);
        }

        [HandlerAction(PacketType.GUILD_GET_ETIN)]
        public async Task HandleGetEtin(WorldClient client, GuildGetEtinPacket packet)
        {
            var etin = 0;
            if (_guildManager.HasGuild)
            {
                etin = await _guildManager.GetEtin();
            }

            _packetFactory.SendGetEtin(client, etin);
        }

        [HandlerAction(PacketType.GUILD_ETIN_RETURN)]
        public async Task HandleEtinReturn(WorldClient client, GuildEtinReturnPacket packet)
        {
            if (!_guildManager.HasGuild)
                return;

            var etins = await _guildManager.ReturnEtin();

            _packetFactory.SendEtinReturnResult(client, etins);
        }

        [HandlerAction(PacketType.GUILD_NPC_UPGRADE)]
        public async Task HandleGuildUpgradeNpc(WorldClient client, GuildNpcUpgradePacket packet)
        {
            if (!_guildManager.HasGuild || (_guildManager.GuildMemberRank != 1 && _guildManager.GuildMemberRank != 2))
            {
                _packetFactory.SendGuildUpgradeNpc(client, GuildNpcUpgradeReason.Failed, packet.NpcType, packet.NpcGroup, packet.NpcLevel);
                return;
            }

            var reason = await _guildManager.TryUpgradeNPC(packet.NpcType, packet.NpcGroup, packet.NpcLevel);
            if (reason == GuildNpcUpgradeReason.Ok)
            {
                var etin = await _guildManager.GetEtin();
                _packetFactory.SendGetEtin(client, etin);
            }

            _packetFactory.SendGuildUpgradeNpc(client, reason, packet.NpcType, packet.NpcGroup, packet.NpcLevel);
        }
    }
}
