using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class NpcBuyItemHandler : BaseHandler
    {
        private readonly ILogger<NpcBuyItemHandler> _logger;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IInventoryManager _inventoryManager;
        private readonly IGuildManager _guildManager;

        public NpcBuyItemHandler(ILogger<NpcBuyItemHandler> logger,IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IGameWorld gameWorld, IInventoryManager inventoryManager, IGuildManager guildManager) : base(packetFactory, gameSession)
        {
            _logger = logger;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _inventoryManager = inventoryManager;
            _guildManager = guildManager;
        }

        [HandlerAction(PacketType.NPC_BUY_ITEM)]
        public void Handle(WorldClient client, NpcBuyItemPacket packet)
        {
            var cellId = _gameWorld.Players[_gameSession.Character.Id].CellId;

            var npc = _mapProvider.Map.GetNPC(cellId, packet.NpcId);
            if (npc is null || !npc.ContainsProduct(packet.ItemIndex))
            {
                _logger.LogWarning("NPC with id {npcId} doesn't contain item at index: {itemIndex}.", packet.NpcId, packet.ItemIndex);
                return;
            }

            var discount = 0f;

            if (_mapProvider.Map is GuildHouseMap)
            {
                if (!_guildManager.HasGuild)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowRank, 30);
                    return;
                }

                var allowed = _guildManager.CanUseNpc(npc.Type, npc.TypeId, out var requiredRank);
                if (!allowed)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowRank, requiredRank);
                    return;
                }

                allowed = _guildManager.HasNpcLevel(npc.Type, npc.TypeId);
                if (!allowed)
                {
                    _packetFactory.SendGuildHouseActionError(client, GuildHouseActionError.LowLevel, 0);
                    return;
                }

                discount = _guildManager.GetDiscount(npc.Type, npc.TypeId);
            }

            var buyItem = npc.Products[packet.ItemIndex];
            var boughtItem = _inventoryManager.BuyItem(buyItem, packet.Count, discount, out var result);
            _packetFactory.SendBoughtItem(client, result, boughtItem, _inventoryManager.Gold);
        }
    }
}
