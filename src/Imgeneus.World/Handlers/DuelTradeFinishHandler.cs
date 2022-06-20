using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DuelTradeFinishHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;
        private readonly IGameWorld _gameWorld;
        private readonly IMovementManager _movementManager;

        public DuelTradeFinishHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager, IGameWorld gameWorld, IMovementManager movementManager) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
            _gameWorld = gameWorld;
            _movementManager = movementManager;
        }

        [HandlerAction(PacketType.DUEL_TRADE_OK)]
        public void Handle(WorldClient client, DuelOkPacket packet)
        {
            _gameWorld.Players.TryGetValue(_duelManager.OpponentId, out var opponent);
            if (opponent is null)
                return;

            if (packet.Result == 0) // ok clicked.
            {
                _duelManager.IsApproved = true;

                _packetFactory.SendDuelApprove(client, 1, _duelManager.IsApproved);
                _packetFactory.SendDuelApprove(opponent.GameSession.Client, 2, _duelManager.IsApproved);

                if (_duelManager.IsApproved && opponent.DuelManager.IsApproved)
                {
                    var x = (_movementManager.PosX + opponent.MovementManager.PosX) / 2;
                    var z = (_movementManager.PosZ + opponent.MovementManager.PosZ) / 2;

                    _duelManager.Ready(x, z);
                    _packetFactory.SendDuelReady(client, x, z);
                    _packetFactory.SendDuelCloseTrade(client, DuelCloseWindowReason.DuelStart);

                    opponent.DuelManager.Ready(x, z);
                    _packetFactory.SendDuelReady(opponent.GameSession.Client, x, z);
                    _packetFactory.SendDuelCloseTrade(opponent.GameSession.Client, DuelCloseWindowReason.DuelStart);
                }
            }
            else if (packet.Result == 1) // ok clicked twice == declined
            {
                _duelManager.IsApproved = false;

                _packetFactory.SendDuelApprove(client, 1, _duelManager.IsApproved);
                _packetFactory.SendDuelApprove(opponent.GameSession.Client, 2, _duelManager.IsApproved);
            }
            else if (packet.Result == 2) // close window was clicked.
            {
                _duelManager.Cancel(_gameSession.Character.Id, DuelCancelReason.Other);
                _packetFactory.SendDuelCloseTrade(client, DuelCloseWindowReason.SenderClosedWindow);
                _packetFactory.SendDuelCloseTrade(opponent.GameSession.Client, DuelCloseWindowReason.OpponentClosedWindow);
            }
        }
    }
}
