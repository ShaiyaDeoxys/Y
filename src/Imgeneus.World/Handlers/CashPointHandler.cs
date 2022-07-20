using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class CashPointHandler : BaseHandler
    {
        private readonly IAdditionalInfoManager _additionalInfoManager;

        public CashPointHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IAdditionalInfoManager additionalInfoManager) : base(packetFactory, gameSession)
        {
            _additionalInfoManager = additionalInfoManager;
        }

        [HandlerAction(PacketType.CASH_POINT)]
        public void Handle(WorldClient client, CashPointPacket packet)
        {
            _packetFactory.SendCashPoint(client, _additionalInfoManager.Points);
        }
    }
}
