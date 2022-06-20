using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class DuelDefeatHandler : BaseHandler
    {
        private readonly IDuelManager _duelManager;

        public DuelDefeatHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IDuelManager duelManager) : base(packetFactory, gameSession)
        {
            _duelManager = duelManager;
        }

        [HandlerAction(PacketType.DUEL_CANCEL)]
        public void Handle(WorldClient client, DuelDefeatPacket packet)
        {
            _duelManager.Cancel(_gameSession.Character.Id, DuelCancelReason.AdmitDefeat);
        }
    }
}
