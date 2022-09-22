using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Packets;
using Parsec.Shaiya.NpcQuest;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MoveItemInInventoryHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IGuildManager _guildManager;

        public MoveItemInInventoryHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, IGuildManager guildManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _guildManager = guildManager;
        }

        [HandlerAction(PacketType.INVENTORY_MOVE_ITEM)]
        public void Handle(WorldClient client, MoveItemInInventoryPacket packet)
        {
            if (packet.CurrentBag == WarehouseManager.GUILD_WAREHOUSE_BAG && _guildManager.GuildMemberRank > 2)
            {
                // Characters of high rank can not take items of guild warehouse.
                return;
            }

            if (packet.DestinationBag == WarehouseManager.GUILD_WAREHOUSE_BAG && _guildManager.GuildMemberRank > 8)
            {
                // Characters with rank 8+ can not store items in guild warehouse.
                return;
            }

            if (packet.DestinationBag == WarehouseManager.GUILD_WAREHOUSE_BAG)
            {
                var level = (byte)(packet.DestinationSlot / 40);
                if(!_guildManager.HasNpcLevel(NpcType.Warehouse, level))
                {
                    // NPC level is less than tab index. Can not use guild warehouse in this case.
                    return;
                }
            }

            var items = _inventoryManager.MoveItem(packet.CurrentBag, packet.CurrentSlot, packet.DestinationBag, packet.DestinationSlot);
            if (items.sourceItem.Bag == WarehouseManager.GUILD_WAREHOUSE_BAG)
            {
                // Looks like there is some bug in client game.exe and bag 255 is not processed correctly.
                items.sourceItem.Bag--;
            }

            _packetFactory.SendMoveItem(client, items.sourceItem, items.destinationItem, _inventoryManager.Gold);
            _packetFactory.SendGoldUpdate(client, _inventoryManager.Gold);
        }
    }
}
