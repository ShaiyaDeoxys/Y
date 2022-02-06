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
    public class TradeFinishHandler : BaseHandler
    {
        private readonly ITradeManager _tradeManager;
        private readonly IGameWorld _gameWorld;

        public TradeFinishHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ITradeManager tradeManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _tradeManager = tradeManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.TRADE_FINISH)]
        public void Handle(WorldClient client, TradeFinishPacket packet)
        {
            if (packet.Result == 2)
            {
                _tradeManager.Cancel();
            }
            else if (packet.Result == 1)
            {
                _tradeManager.ConfirmDeclined();

                // Decline both.
                _packetFactory.SendTradeConfirm(client, 1, true);
                _packetFactory.SendTradeConfirm(client, 2, true);
                _packetFactory.SendTradeConfirm(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 1, true);
                _packetFactory.SendTradeConfirm(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, true);
                
            }
            else if (packet.Result == 0)
            {
                _tradeManager.Confirmed();

                // 1 means sender, 2 means partner.
                _packetFactory.SendTradeConfirm(client, 1, false);
                _packetFactory.SendTradeConfirm(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client, 2, false);

                if (_tradeManager.Request.IsConfirmed_1 && _tradeManager.Request.IsConfirmed_2)
                {
                    _tradeManager.FinishSuccessful();

                    _packetFactory.SendTradeFinished(client);
                    _packetFactory.SendTradeFinished(_gameWorld.Players[_tradeManager.PartnerId].GameSession.Client);
                }
            }
        }
    }
}
