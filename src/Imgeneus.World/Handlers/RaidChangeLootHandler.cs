using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidChangeLootHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;

        public RaidChangeLootHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
        }

        [HandlerAction(PacketType.RAID_CHANGE_LOOT)]
        public void Handle(WorldClient client, RaidChangeLootPacket packet)
        {
            if (!_partyManager.IsPartyLead || _partyManager.Party is not Raid)
                return;

            (_partyManager.Party as Raid).ChangeDropType((RaidDropType)packet.LootType);
        }
    }
}
