using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.PartyAndRaid
{
    /// <summary>
    /// Party manager handles all party packets.
    /// </summary>
    public class PartyManager : IPartyManager
    {
        private readonly ILogger<PartyManager> _logger;
        private readonly IGamePacketFactory _packetFactory;
        private readonly IGameWorld _gameWorld;
        private readonly IMapProvider _mapProvider;
        private readonly IHealthManager _healthManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IBlessManager _blessManager;
        private uint _ownerId;

        public PartyManager(ILogger<PartyManager> logger, IGamePacketFactory packetFactory, IGameWorld gameWorld, IMapProvider mapProvider, IHealthManager healthManager, ICountryProvider countryProvider, IBlessManager blessManager)
        {
            _logger = logger;
            _packetFactory = packetFactory;
            _gameWorld = gameWorld;
            _mapProvider = mapProvider;
            _healthManager = healthManager;
            _countryProvider = countryProvider;
            _blessManager = blessManager;
            _summonTimer.Elapsed += OnSummonningFinished;
            _healthManager.OnGotDamage += CancelSummon;

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

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public Task Clear()
        {
            Party = null;
            _player = null;
            SetSummonAnswer(false);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _summonTimer.Elapsed -= OnSummonningFinished;
            _healthManager.OnGotDamage -= CancelSummon;
        }

        #endregion

        #region Party creation

        public uint InviterId { get; set; }

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

        #region Summon

        private Timer _summonTimer = new Timer() { AutoReset = false };

        public event Action<uint> OnSummonning;

        public event Action<uint> OnSummoned;

        public bool IsSummoning { get; set; }

        public void SummonMembers(bool skeepTimer = false, Item summonItem = null)
        {
            if (!HasParty)
                return;

            Party.SummonRequest = new SummonRequest(_ownerId, summonItem);
            IsSummoning = true;

            if (skeepTimer)
                OnSummonningFinished(null, null);
            else
            {
                _summonTimer.Interval = 5000;

                if (_countryProvider.Country == CountryType.Light && _blessManager.LightAmount >= IBlessManager.CAST_TIME_DISPOSABLE_ITEMS)
                    _summonTimer.Interval = 4500;
                if (_countryProvider.Country == CountryType.Dark && _blessManager.DarkAmount >= IBlessManager.CAST_TIME_DISPOSABLE_ITEMS)
                    _summonTimer.Interval = 4500;

                _summonTimer.Start();
            }

            OnSummonning?.Invoke(_ownerId);
        }

        private void OnSummonningFinished(object sender, ElapsedEventArgs e)
        {
            if (HasParty && IsSummoning)
            {

                foreach (var member in Party.GetShortMembersList(_gameWorld.Players[_ownerId]).Where(x => x.Id != _ownerId))
                {
                    Party.SummonRequest.MemberAnswers[member.Id] = null;
                    _packetFactory.SendPartycallRequest(member.GameSession.Client, Party.SummonRequest.OwnerId);
                }

                OnSummoned?.Invoke(_ownerId);
            }

            IsSummoning = false;
        }

        private void CancelSummon(uint senderId, IKiller damageMaker, int damage)
        {
            if (Party is not null && Party.SummonRequest is not null)
            {
                Party.SummonRequest = null;
                IsSummoning = false;
                _summonTimer.Stop();
            }
        }

        public void SetSummonAnswer(bool isOk)
        {
            if (!HasParty || Party.SummonRequest is null || !Party.SummonRequest.MemberAnswers.ContainsKey(_ownerId) || Party.SummonRequest.MemberAnswers[_ownerId] is not null)
                return;

            Party.SummonRequest.MemberAnswers[_ownerId] = isOk;

            // If all answred request can be cleared.
            if (Party.SummonRequest.MemberAnswers.All(x => x.Value is not null))
                Party.SummonRequest = null;
        }

        #endregion

        #region Helpers

        private Character _player;
        private Character Player
        {
            get
            {
                if (_player is null)
                    _player = _gameWorld.Players[_ownerId];

                return _player;
            }
        }

        public bool HasParty { get => Party != null; }

        public bool IsPartyLead { get => Party != null && Party.Leader == Player; }

        public bool IsPartySubLeader { get => Party != null && Party.SubLeader == Player; }

        #endregion
    }
}
