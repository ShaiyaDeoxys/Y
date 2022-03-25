using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class BankClaimItemHandler : BaseHandler
    {
        private readonly IBankManager _bankManager;

        public BankClaimItemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IBankManager bankManager) : base(packetFactory, gameSession)
        {
            _bankManager = bankManager;
        }

        [HandlerAction(PacketType.BANK_CLAIM_ITEM)]
        public void Handle(WorldClient client, BankClaimItemPacket packet)
        {
            var item = _bankManager.TryClaimBankItem(packet.Slot);
            if (item is null)
                _packetFactory.SendFullInventory(client);
            else
            {
                _packetFactory.SendBankItemClaim(client, packet.Slot, item);

                if (item.IsExpirable)
                    _packetFactory.SendItemExpiration(client, item);
            }
        }
    }
}
