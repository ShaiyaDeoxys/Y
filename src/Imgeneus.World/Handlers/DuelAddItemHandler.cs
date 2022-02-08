using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DuelAddItemHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;
        private readonly IGameWorld _gameWorld;

        public DuelAddItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.DUEL_TRADE_ADD_ITEM)]
        public void Handle(WorldClient client, DuelAddItemPacket packet)
        {
            var ok = _duelManager.TryAddItem(packet.Bag, packet.Slot, packet.Quantity, packet.SlotInTradeWindow, out var tradeItem);
            if (ok)
            {
                _packetFactory.SendDuelAddItem(client, packet.Bag, packet.Slot, packet.Quantity, packet.SlotInTradeWindow);
                _packetFactory.SendDuelAddItem(_gameWorld.Players[_duelManager.OpponentId].GameSession.Client, tradeItem, packet.Quantity, packet.SlotInTradeWindow);
            }
        }
    }
}
