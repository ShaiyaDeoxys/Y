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

        public Task Clear()
        {
            Party = null;

            return Task.CompletedTask;
        }

        #endregion

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
