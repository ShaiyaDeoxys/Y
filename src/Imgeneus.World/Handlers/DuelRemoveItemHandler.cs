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
    public class DuelRemoveItemHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;
        private readonly IGameWorld _gameWorld;

        public DuelRemoveItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.DUEL_TRADE_REMOVE_ITEM)]
        public void Handle(WorldClient client, DuelRemoveItemPacket packet)
        {
            var ok = _duelManager.TryRemoveItem(packet.SlotInTradeWindow);
            if (ok)
            {
                _packetFactory.SendDuelRemoveItem(client, packet.SlotInTradeWindow, 1);
                _packetFactory.SendDuelRemoveItem(_gameWorld.Players[_duelManager.OpponentId].GameSession.Client, packet.SlotInTradeWindow, 2);
            }
        }
    }
}
