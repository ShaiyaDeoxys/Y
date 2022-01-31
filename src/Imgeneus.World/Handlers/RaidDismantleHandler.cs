using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidDismantleHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public RaidDismantleHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.RAID_DISMANTLE)]
        public void Handle(WorldClient client, RaidDismantlePacket packet)
        {
            if (!_partyManager.IsPartyLead || _partyManager.Party is not Raid)
                return;

            _partyManager.Party.Dismantle();
        }
    }
}
