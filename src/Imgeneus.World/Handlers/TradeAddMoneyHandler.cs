using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class TradeAddMoneyHandler : BaseHandler
    {
        private readonly ITradeManager _tradeManager;
        private readonly IGameWorld _gameWorld;

        public TradeAddMoneyHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITradeManager tradeManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _tradeManager = tradeManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_ADD_MONEY)]
        public void Handle(WorldClient client, TradeAddMoneyPacket packet)
        {
            var ok = _tradeManager.TryAddMoney(packet.Money, out var money);
            if (ok)
            {
                _packetFactory.SendAddedMoneyToTrade(client, 1, money);
                _packetFactory.SendAddedMoneyToTrade(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, money);
            }
        }
    }
}
