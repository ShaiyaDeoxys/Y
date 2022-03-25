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
    public class DuelAddMoneyHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;
        private readonly IGameWorld _gameWorld;

        public DuelAddMoneyHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.DUEL_TRADE_ADD_MONEY)]
        public void Handle(WorldClient client, DuelAddMoneyPacket packet)
        {
            var ok = _duelManager.TryAddMoney(packet.Money, out var money);
            if (ok)
            {
                _packetFactory.SendDuelAddMoney(client, 1, money);
                _packetFactory.SendDuelAddMoney(_gameWorld.Players[_duelManager.OpponentId].GameSession.Client, 2, money);
            }
        }
    }
}
