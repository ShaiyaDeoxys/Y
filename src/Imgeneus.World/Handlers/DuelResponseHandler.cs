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
    public class DuelResponseHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;
        private readonly IDuelManager _duelManager;

        public DuelResponseHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld, IDuelManager duelManager) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
            _duelManager = duelManager;
        }

        [HandlerAction(PacketType.DUEL_RESPONSE)]
        public void Handle(WorldClient client, DuelResponsePacket packet)
        {
            _gameWorld.Players.TryGetValue(_duelManager.OpponentId, out var opponent);
            if (opponent is null)
                return;

            _duelManager.ProcessResponse(_gameSession.Character.Id, packet.IsDuelApproved ? DuelResponse.Approved : DuelResponse.Rejected);

            opponent.DuelManager.ProcessResponse(_gameSession.Character.Id, packet.IsDuelApproved ? DuelResponse.Approved : DuelResponse.Rejected);

            if (packet.IsDuelApproved)
            {
                _packetFactory.SendDuelStartTrade(client, _duelManager.OpponentId);
                _packetFactory.SendDuelStartTrade(opponent.GameSession.Client, opponent.DuelManager.OpponentId);
            }

        }
    }
}
