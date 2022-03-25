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
            _gameWorld.Players.TryGetValue(_tradeManager.PartnerId, out var patner);
            if (patner is null)
                return;

            if (packet.Result == 2)
            {
                _tradeManager.Cancel();
                patner.TradeManager.Cancel();

                _packetFactory.SendTradeCanceled(client);
                _packetFactory.SendTradeCanceled(patner.GameSession.Client);
            }
            else if (packet.Result == 1)
            {
                _tradeManager.ConfirmDeclined();

                // Decline both.
                _packetFactory.SendTradeConfirm(client, 1, true);
                _packetFactory.SendTradeConfirm(client, 2, true);
                _packetFactory.SendTradeConfirm(patner.GameSession.Client, 1, true);
                _packetFactory.SendTradeConfirm(patner.GameSession.Client, 2, true);
                
            }
            else if (packet.Result == 0)
            {
                _tradeManager.Confirmed();

                // 1 means sender, 2 means partner.
                _packetFactory.SendTradeConfirm(client, 1, false);
                _packetFactory.SendTradeConfirm(patner.GameSession.Client, 2, false);

                if (_tradeManager.Request.IsConfirmed_1 && _tradeManager.Request.IsConfirmed_2)
                {
                    _tradeManager.FinishSuccessful();
                    _packetFactory.SendTradeFinished(client);

                    patner.TradeManager.FinishSuccessful();
                    _packetFactory.SendTradeFinished(patner.GameSession.Client);
                }
            }
        }
    }
}
