using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RaidInviteHandler : BaseHandler
    {
        private readonly IPartyManager _partyManager;
        private readonly IGameWorld _gameWorld;

        public RaidInviteHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IPartyManager partyManager, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _partyManager = partyManager;
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.RAID_INVITE)]
        public void Handle(WorldClient client, RaidInvitePacket packet)
        {
            if (_partyManager.Party is not Raid || (!_partyManager.IsPartyLead && !_partyManager.IsPartySubLeader))
                return;

            if (_gameWorld.Players.TryGetValue(packet.CharacterId, out var requestedPlayer) && _gameSession.Character.Id != 0)
            {
                requestedPlayer.PartyManager.InviterId = _gameSession.Character.Id;
                _packetFactory.SendRaidInvite(requestedPlayer.GameSession.Client, _gameSession.Character.Id);
            }
        }
    }
}
