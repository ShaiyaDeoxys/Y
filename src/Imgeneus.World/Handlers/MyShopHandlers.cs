using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shop;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MyShopHandlers : BaseHandler
    {
        private readonly IShopManager _shopManager;

        public MyShopHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IShopManager shopManager) : base(packetFactory, gameSession)
        {
            _shopManager = shopManager;
        }

        [HandlerAction(PacketType.MY_SHOP_BEGIN)]
        public void HandleBegin(WorldClient client, MyShopBeginPacket packet)
        {
            _shopManager.Begin();
            _packetFactory.SendMyShopBegin(client);
        }

        [HandlerAction(PacketType.MY_SHOP_ADD_ITEM)]
        public void HandleAddItem(WorldClient client, MyShopAddItemPacket packet)
        {
            var ok = _shopManager.TryAddItem(packet.Bag, packet.Slot, packet.ShopSlot, packet.Price);
            if (ok)
                _packetFactory.SendMyShopAddItem(client, packet.Bag, packet.Slot, packet.ShopSlot, packet.Price);
        }

        [HandlerAction(PacketType.MY_SHOP_REMOVE_ITEM)]
        public void HandleRemoveItem(WorldClient client, MyShopRemoveItemPacket packet)
        {
            var ok = _shopManager.TryRemoveItem(packet.ShopSlot);
            if (ok)
                _packetFactory.SendMyShopRemoveItem(client, packet.ShopSlot);
        }
    }
}
