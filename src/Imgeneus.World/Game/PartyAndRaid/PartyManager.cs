using Imgeneus.Network.Data;
using Imgeneus.Network.Packets;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.PartyAndRaid
{
    /// <summary>
    /// Party manager handles all party packets.
    /// </summary>
    public class PartyManager : IPartyManager
    {
        private readonly ILogger<PartyManager> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly IMapProvider _mapProvider;

        private int _ownerId;

        public PartyManager(ILogger<PartyManager> logger, IGameWorld gameWorld, IMapProvider mapProvider)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _mapProvider = mapProvider;
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

        #region Init & Clear

        public void Init(int ownerId)
        {
            _ownerId = ownerId;
        }

        public async Task Clear()
        {
            Party = null;
        }

        #endregion

        /*private void Client_OnPacketArrived(ServerClient sender, IDeserializedPacket packet)
        {
            var worldSender = (IWorldClient)sender;

            switch (packet)
            {
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

        private IParty _party;

        public IParty Party
        {
            get => _party;
            set
            {
                if (_party != null)
                    _party.OnLeaderChanged -= Party_OnLeaderChanged;

                // Leave party.
                if (_party != null && value is null)
                {
                    if (_party.Members.Contains(Player)) // When the player is kicked of the party, the party doesn't contain him.
                        _party.LeaveParty(Player);
                    PreviousPartyId = _party.Id;
                    _party = value;
                }
                // Enter party
                else if (value != null)
                {
                    if (value.EnterParty(Player))
                    {
                        _party = value;

                        //if (_party is Raid)
                        //    _packetsHelper.SendRaidInfo(Client, Party as Raid);

                        _party.OnLeaderChanged += Party_OnLeaderChanged;
                        _mapProvider.Map.UnregisterSearchForParty(Player);
                    }
                }

                OnPartyChanged?.Invoke(Player);
            }
        }

        public Guid PreviousPartyId { get; set; } = Guid.NewGuid();

        #endregion

        #region Party change

        public event Action<Character> OnPartyChanged;

        private void Party_OnLeaderChanged(Character oldLeader, Character newLeader)
        {
            if (Player == oldLeader || Player == newLeader)
                OnPartyChanged?.Invoke(Player);
        }

        #endregion

        #region Helpers

        private Character Player { get => _gameWorld.Players[_ownerId]; }

        public bool HasParty { get => Party != null; }

        public bool IsPartyLead { get => Party != null && Party.Leader == Player; }

        public bool IsPartySubLeader { get => Party != null && Party.SubLeader == Player; }

        #endregion
    }
}
