using Imgeneus.Network.Data;
using Imgeneus.Network.Packets;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.PartyAndRaid
{
    /// <summary>
    /// Party manager handles all party packets.
    /// </summary>
    public class PartyManager : IPartyManager
    {
        private readonly ILogger<PartyManager> _logger;
        private readonly IGameWorld _gameWorld;

        public PartyManager(ILogger<PartyManager> logger, IGameWorld gameWorld)
        {
            _logger = logger;
            _gameWorld = gameWorld;
#if DEBUG
            _logger.LogDebug("PartyManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~PartyManager()
        {
            _logger.LogDebug("PartyManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        /*private void Client_OnPacketArrived(ServerClient sender, IDeserializedPacket packet)
        {
            var worldSender = (IWorldClient)sender;

            switch (packet)
            {
                case PartyResponsePacket responsePartyPacket:
                    if (_gameWorld.Players.TryGetValue(responsePartyPacket.CharacterId, out var partyResponser))
                    {
                        if (responsePartyPacket.IsDeclined)
                        {
                            if (_gameWorld.Players.TryGetValue(partyResponser.PartyInviterId, out var partyRequester))
                            {
                                SendDeclineParty(partyRequester.Client, worldSender.CharID);
                            }
                        }
                        else
                        {
                            if (_gameWorld.Players.TryGetValue(partyResponser.PartyInviterId, out var partyRequester))
                            {
                                if (partyRequester.Party is null)
                                {
                                    var party = new Party();
                                    partyRequester.SetParty(party);
                                    partyResponser.SetParty(party);
                                }
                                else
                                {
                                    partyResponser.SetParty(partyRequester.Party);
                                }
                            }
                        }

                        partyResponser.PartyInviterId = 0;
                    }
                    break;

                case PartyLeavePacket partyLeavePacket:
                    _player.SetParty(null);
                    break;

                case PartyKickPacket partyKickPacket:
                    if (!_player.IsPartyLead)
                        return;

                    var playerToKick = _player.Party.Members.FirstOrDefault(m => m.Id == partyKickPacket.CharacterId);
                    if (playerToKick != null)
                    {
                        _player.Party.KickMember(playerToKick);
                        playerToKick.SetParty(null, true);
                    }
                    break;

                case PartyChangeLeaderPacket changeLeaderPacket:
                    if (!_player.IsPartyLead)
                        return;

                    var newLeader = _player.Party.Members.FirstOrDefault(m => m.Id == changeLeaderPacket.CharacterId);
                    if (newLeader != null)
                        _player.Party.Leader = newLeader;
                    break;

                case PartySearchInvitePacket partySearchInvitePacket:
                    var requestedPlayer = _player.Map.PartySearchers.FirstOrDefault(p => p.Name == partySearchInvitePacket.Name);
                    if (requestedPlayer != null && requestedPlayer.Party is null)
                        RequestParty(worldSender.CharID, requestedPlayer.Id);
                    break;

                case RaidCreatePacket raidCreatePacket:
                    if (!_player.IsPartyLead)
                        return;
                    var raid = new Raid(raidCreatePacket.AutoJoin, (RaidDropType)raidCreatePacket.DropType);
                    var members = _player.Party.Members.ToList();
                    foreach (var member in members)
                    {
                        member.SetParty(raid, true);
                    }
                    raid.Leader = _player;
                    foreach (var m in members)
                    {
                        SendRaidCreated(m.Client, raid);
                    }
                    break;

                case RaidDismantlePacket raidDismantlePacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    _player.Party.Dismantle();
                    break;

                case RaidLeavePacket raidLeavePacket:
                    _player.SetParty(null);
                    break;

                case RaidChangeAutoInvitePacket raidChangeAutoInvitePacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    (_player.Party as Raid).ChangeAutoJoin(raidChangeAutoInvitePacket.IsAutoInvite);
                    break;

                case RaidChangeLootPacket raidChangeLootPacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    (_player.Party as Raid).ChangeDropType((RaidDropType)raidChangeLootPacket.LootType);
                    break;

                case RaidJoinPacket raidJoinPacket:
                    if (_player.Party != null) // Player is already in party.
                    {
                        SendPartyError(_player.Client, PartyErrorType.RaidNotFound);
                        return;
                    }

                    var raidMember = _gameWorld.Players.Values.FirstOrDefault(m => m.Name == raidJoinPacket.CharacterName);
                    if (raidMember is null || raidMember.Country != _player.Country || !(raidMember.Party is Raid))
                        SendPartyError(_player.Client, PartyErrorType.RaidNotFound);
                    else
                    {
                        if ((raidMember.Party as Raid).AutoJoin)
                        {
                            _player.SetParty(raidMember.Party);
                            if (_player.Party is null)
                            {
                                SendPartyError(_player.Client, PartyErrorType.RaidNoFreePlace);
                            }
                        }
                        else
                        {
                            SendPartyError(_player.Client, PartyErrorType.RaidNoAutoJoin);
                        }
                    }
                    break;

                case RaidChangeLeaderPacket raidChangeLeaderPacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    if (!_gameWorld.Players.TryGetValue(raidChangeLeaderPacket.CharacterId, out var newRaidLeader))
                        return;
                    if (newRaidLeader.Party != _player.Party)
                        return;
                    _player.Party.Leader = newRaidLeader;
                    break;

                case RaidChangeSubLeaderPacket raidChangeSubLeaderPacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    if (!_gameWorld.Players.TryGetValue(raidChangeSubLeaderPacket.CharacterId, out var newRaidSubLeader))
                        return;
                    if (newRaidSubLeader.Party != _player.Party)
                        return;
                    _player.Party.SubLeader = newRaidSubLeader;
                    break;

                case RaidKickPacket raidKickPacket:
                    if (!_player.IsPartyLead || !(_player.Party is Raid))
                        return;
                    if (!_gameWorld.Players.TryGetValue(raidKickPacket.CharacterId, out var kickMember))
                        return;
                    if (kickMember.Party != _player.Party)
                        return;
                    _player.Party.KickMember(kickMember);
                    kickMember.SetParty(null, true);
                    break;

                case RaidMovePlayerPacket raidMovePlayerPacket:
                    if (!(_player.Party is Raid) || (!_player.IsPartyLead && !_player.IsPartySubLeader))
                        return;
                    (_player.Party as Raid).MoveCharacter(raidMovePlayerPacket.SourceIndex, raidMovePlayerPacket.DestinationIndex);
                    break;
            }
        }*/

        #region Party creation

        public int InviterId { get; set; }

        #endregion

        private void SendPartyError(IWorldClient client, PartyErrorType partyError, int id = 0)
        {
            using var packet = new Packet(PacketType.RAID_PARTY_ERROR);
            packet.Write((int)partyError);
            packet.Write(id);
            //client.SendPacket(packet);
        }

        private void SendDeclineParty(IWorldClient client, int charID)
        {
            using var packet = new Packet(PacketType.PARTY_RESPONSE);
            packet.Write(false);
            packet.Write(charID);
            //client.SendPacket(packet);
        }

        private void SendRaidCreated(IWorldClient client, Raid raid)
        {
            using var packet = new Packet(PacketType.RAID_CREATE);
            packet.Write(true); // raid type ?
            packet.Write(raid.AutoJoin);
            packet.Write((int)raid.DropType);
            packet.Write(raid.GetIndex(raid.Leader));
            packet.Write(raid.GetIndex(raid.SubLeader));
            //client.SendPacket(packet);
        }
    }
}
