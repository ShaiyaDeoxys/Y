using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidCreateHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;

        public RaidCreateHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.RAID_CREATE)]
        public void Handle(WorldClient client, RaidCreatePacket packet)
        {
            if (!_partyManager.IsPartyLead)
                return;

            var raid = new Raid(packet.AutoJoin, (RaidDropType)packet.DropType, _packetFactory);
            var members = _partyManager.Party.Members.ToList();
            foreach (var member in members)
                member.PartyManager.Party = raid;

            raid.Leader = _gameWorld.Players[_gameSession.Character.Id];
            foreach (var m in members)
                _packetFactory.SendRaidCreated(m.GameSession.Client, raid);
        }
    }
}
