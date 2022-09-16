using Imgeneus.Database;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Imgeneus.World.SelectionScreen;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AccountFractionHandler : BaseHandler
    {
        private readonly ISelectionScreenManager _selectionScreenManager;

        public AccountFractionHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ISelectionScreenManager selectionScreenManager) : base(packetFactory, gameSession)
        {
            _selectionScreenManager = selectionScreenManager;
        }

        [HandlerAction(PacketType.ACCOUNT_FACTION)]
        public async Task Handle(WorldClient client, AccountFractionPacket packet)
        {
            await _selectionScreenManager.SetFaction(client.UserId, packet.Fraction);
            var mode = await _selectionScreenManager.GetMaxMode(client.UserId);

            _packetFactory.SendFaction(client, packet.Fraction, mode);
        }
    }
}
