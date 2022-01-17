using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.DatabaseBackgroundService;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.Zone.MapConfig;
using System.Collections.Concurrent;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Country;

namespace Imgeneus.World.Game.Player
{
    public partial class Character : BaseKillable, IKiller, IMapMember, IDisposable
    {
        private readonly ILogger<Character> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly ICharacterConfiguration _characterConfig;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IMapsLoader _mapLoader;
        private readonly PacketsHelper _packetsHelper;
        private readonly IChatManager _chatManager;
        private readonly ILinkingManager _linkingManager;
        private readonly IDyeingManager _dyeingManager;
        private readonly IMobFactory _mobFactory;
        private readonly INpcFactory _npcFactory;
        private readonly INoticeManager _noticeManager;
        private readonly IGuildManager _guildManager;

        public IInventoryManager InventoryManager { get; private set; }
        public IStealthManager StealthManager { get; private set; }
        public ILevelingManager LevelingManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public ISkillsManager SkillsManager { get; private set; }
        public IGameSession GameSession { get; private set; }

        public Character(ILogger<Character> logger,
                         IGameWorld gameWorld,
                         ICharacterConfiguration characterConfig,
                         IBackgroundTaskQueue taskQueue,
                         IDatabasePreloader databasePreloader,
                         IMapsLoader mapLoader,
                         IChatManager chatManager,
                         ILinkingManager linkinManager,
                         IDyeingManager dyeingManager,
                         IMobFactory mobFactory,
                         INpcFactory npcFactory,
                         INoticeManager noticeManager,
                         IGuildManager guildManager,
                         ICountryProvider countryProvider,
                         IStatsManager statsManager,
                         IHealthManager healthManager,
                         ILevelProvider levelProvider,
                         ILevelingManager levelingManager,
                         IInventoryManager inventoryManager,
                         IStealthManager stealthManager,
                         IAttackManager attackManager,
                         ISkillsManager skillsManager,
                         IBuffsManager buffsManager,
                         IElementProvider elementProvider,
                         IGameSession gameSession) : base(databasePreloader, countryProvider, statsManager, healthManager, levelProvider, buffsManager, elementProvider)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _characterConfig = characterConfig;
            _taskQueue = taskQueue;
            _mapLoader = mapLoader;
            _chatManager = chatManager;
            _linkingManager = linkinManager;
            _dyeingManager = dyeingManager;
            _mobFactory = mobFactory;
            _npcFactory = npcFactory;
            _noticeManager = noticeManager;
            _guildManager = guildManager;

            InventoryManager = inventoryManager;
            StealthManager = stealthManager;
            LevelingManager = levelingManager;
            AttackManager = attackManager;
            SkillsManager = skillsManager;
            GameSession = gameSession;

            StatsManager.OnAdditionalStatsUpdate += SendAdditionalStats;
            BuffsManager.OnBuffAdded += OnBuffAdded;
            BuffsManager.OnBuffRemoved += OnBuffRemoved;
            AttackManager.OnStartAttack += SendAttackStart;

            _packetsHelper = new PacketsHelper();

            _summonVehicleTimer.Elapsed += SummonVehicleTimer_Elapsed;

            OnDead += Character_OnDead;

            Bless.Instance.OnDarkBlessChanged += OnDarkBlessChanged;
            Bless.Instance.OnLightBlessChanged += OnLightBlessChanged;
        }

        private void Init()
        {
            InitPassiveSkills();
            InitQuests();

            // Send notification to friends.
            foreach (var friend in Friends.Values)
            {
                _gameWorld.Players.TryGetValue(friend.Id, out var player);

                if (player != null)
                    player.FriendOnline(this);
            }
        }

        public void Dispose()
        {
            if (Party != null)
                SetParty(null);

            StatsManager.OnAdditionalStatsUpdate -= SendAdditionalStats;
            BuffsManager.OnBuffAdded -= OnBuffAdded;
            BuffsManager.OnBuffRemoved -= OnBuffRemoved;
            AttackManager.OnStartAttack -= SendAttackStart;

            _summonVehicleTimer.Elapsed -= SummonVehicleTimer_Elapsed;

            OnDead -= Character_OnDead;

            Bless.Instance.OnDarkBlessChanged -= OnDarkBlessChanged;
            Bless.Instance.OnLightBlessChanged -= OnLightBlessChanged;

            // Notify friends, that player is offline.
            foreach (var friend in Friends.Values)
            {
                _gameWorld.Players.TryGetValue(friend.Id, out var friendPlayer);
                if (friendPlayer != null)
                    friendPlayer.FriendOffline(this);
            }

            // Notify guild members, that player is offline.
            NotifyGuildMembersOffline();

            // Save current quests state to database.
            foreach (var quest in Quests.Where(q => q.SaveUpdateToDatabase))
            {
                _taskQueue.Enqueue(ActionType.QUEST_UPDATE, Id, quest.Id, quest.RemainingTime, quest.CountMob1, quest.CountMob2, quest.Count3, quest.IsFinished, quest.IsSuccessful);
                quest.QuestTimeElapsed -= Quest_QuestTimeElapsed;
            }

            // Save current HP, MP, SP to database.
            _taskQueue.Enqueue(ActionType.SAVE_CHARACTER_HP_MP_SP, Id, HealthManager.CurrentHP, HealthManager.CurrentMP, HealthManager.CurrentSP);

            Map = null;

            ClearConnection();
        }

        private void OnBuffAdded(int senderId, Buff buff) => SendAddBuff(buff);

        private void OnBuffRemoved(int senderId, Buff buff) => SendRemoveBuff(buff);

        #region Run mode

        /// <summary>
        ///  Set to 1 if you want character running or to 0 if character is "walking".
        ///  Used to change with Tab in previous episodes.
        /// </summary>
        public byte MoveMotion
        {
            get
            {
                if (BuffsManager.ActiveBuffs.Any(b => b.StateType == StateType.Immobilize || b.StateType == StateType.Sleep || b.StateType == StateType.Stun))
                {
                    return 193; // Can not move motion.
                }

                if (StealthManager.IsStealth)
                    return 0;

                return 1;
            }
        }

        #endregion

        #region Motion

        /// <summary>
        /// Event, that is fires, when character makes any motion.
        /// </summary>
        public event Action<Character, Motion> OnMotion;

        /// <summary>
        /// Motion, like sit.
        /// </summary>
        private Motion _motion;
        public Motion Motion
        {
            get => _motion;
            set
            {
                _logger.LogDebug($"Character {Id} sends motion {value}");

                if (value == Motion.None || value == Motion.Sit)
                {
                    _motion = value;
                }
                
                OnMotion?.Invoke(this, value);
            }
        }

        #endregion

        #region Quick skill bar

        /// <summary>
        /// Quick items, i.e. skill bars. Not sure if I need to store it as DbQuickSkillBarItem or need another connector helper class here?
        /// </summary>
        public IEnumerable<DbQuickSkillBarItem> QuickItems;

        #endregion

        #region Trade

        /// <summary>
        /// With whom player is currently trading.
        /// </summary>
        public Character TradePartner;

        /// <summary>
        /// Represents currently open trade window.
        /// </summary>
        public TradeRequest TradeRequest;

        /// <summary>
        /// Items, that are currently in trade window.
        /// </summary>
        public ConcurrentDictionary<byte, Item> TradeItems = new ConcurrentDictionary<byte, Item>();

        /// <summary>
        /// Money in trade window.
        /// </summary>
        public uint TradeMoney;

        /// <summary>
        /// Clears trade items and gold.
        /// </summary>
        public void ClearTrade()
        {
            TradeItems.Clear();
            TradeMoney = 0;
            TradeRequest = null;
            TradePartner = null;
        }

        #endregion

        #region Duel

        /// <summary>
        /// Duel opponent.
        /// </summary>
        public Character DuelOpponent;

        /// <summary>
        /// Indicator, that shows if a player has answered duel request.
        /// </summary>
        public bool AnsweredDuelRequest;

        /// <summary>
        /// Indicator, that shows if a player has clicked "ok" in trade window of duel.
        /// </summary>
        public bool IsDuelApproved;

        /// <summary>
        /// Duel x position start.
        /// </summary>
        public float DuelX;

        /// <summary>
        /// Duel z position start.
        /// </summary>
        public float DuelZ;

        /// <summary>
        /// Finishes duel, because of any reason.
        /// </summary>
        public event Action<DuelCancelReason> OnDuelFinish;

        /// <summary>
        /// Finishes duel.
        /// </summary>
        /// <param name="reason">Reason why duel was finished.</param>
        private void FinishDuel(DuelCancelReason reason)
        {
            if (IsDuelApproved)
            {
                if (reason == DuelCancelReason.Lose || reason == DuelCancelReason.AdmitDefeat)
                {
                    SetDefeats(++Defeats);
                    DuelOpponent.SetVictories(++DuelOpponent.Victories);
                }
                OnDuelFinish?.Invoke(reason);
            }
        }

        #endregion

        #region Death

        private void Character_OnDead(IKillable sender, IKiller killer)
        {
            if (IsDuelApproved && killer == DuelOpponent)
                FinishDuel(DuelCancelReason.Lose);
        }

        #endregion

        #region Overrides

        /*protected override void DecreaseHP(IKiller damageMaker)
        {
            StealthManager.IsStealth = false;
            IsOnVehicle = false;
        }*/

        #endregion

        /// <summary>
        /// Creates character from database information.
        /// </summary>
        public static Character FromDbCharacter(DbCharacter dbCharacter, ILogger<Character> logger, IGameWorld gameWorld, ICharacterConfiguration characterConfig, IBackgroundTaskQueue taskQueue, IDatabasePreloader databasePreloader, IMapsLoader mapsLoader, ICountryProvider countryProvider, IStatsManager statsManager, IHealthManager healthManager, ILevelProvider levelProvider, ILevelingManager levelingManager, IInventoryManager inventoryManager, IChatManager chatManager, ILinkingManager linkingManager, IDyeingManager dyeingManager, IMobFactory mobFactory, INpcFactory npcFactory, INoticeManager noticeManager, IGuildManager guildManger, IStealthManager stealthManager, IAttackManager attackManager, ISkillsManager skillsManager, IBuffsManager buffsManager, IElementProvider elementProvider, IGameSession gameSession)
        {
            var character = new Character(logger, gameWorld, characterConfig, taskQueue, databasePreloader, mapsLoader, chatManager, linkingManager, dyeingManager, mobFactory, npcFactory, noticeManager, guildManger, countryProvider, statsManager, healthManager, levelProvider, levelingManager, inventoryManager, stealthManager, attackManager, skillsManager, buffsManager, elementProvider, gameSession)
            {
                Id = dbCharacter.Id,
                Name = dbCharacter.Name,
                MapId = dbCharacter.Map,
                Race = dbCharacter.Race,
                Class = dbCharacter.Class,
                Hair = dbCharacter.Hair,
                Face = dbCharacter.Face,
                Height = dbCharacter.Height,
                Gender = dbCharacter.Gender,
                PosX = dbCharacter.PosX,
                PosY = dbCharacter.PosY,
                PosZ = dbCharacter.PosZ,
                Angle = dbCharacter.Angle,
                AutoStr = dbCharacter.AutoStr,
                AutoDex = dbCharacter.AutoDex,
                AutoInt = dbCharacter.AutoInt,
                AutoRec = dbCharacter.AutoRec,
                AutoWis = dbCharacter.AutoWis,
                AutoLuc = dbCharacter.AutoLuc,
                Exp = dbCharacter.Exp,
                Kills = dbCharacter.Kills,
                Deaths = dbCharacter.Deaths,
                Victories = dbCharacter.Victories,
                Defeats = dbCharacter.Defeats,
                IsAdmin = dbCharacter.User.Authority == 0,
                Points = dbCharacter.User.Points,
                GuildId = dbCharacter.GuildId
            };

            var quests = dbCharacter.Quests.Select(q => new Quest(databasePreloader, q)).ToList();
            character.Quests.AddRange(quests);

            character.QuickItems = dbCharacter.QuickItems;

            //foreach (var friend in dbCharacter.Friends.Select(f => f.Friend))
            //    character.Friends.TryAdd(friend.Id, new Friend(friend.Id, friend.Name, friend.Class, gameWorld.Players.ContainsKey(friend.Id)));

            foreach (var bankItem in dbCharacter.User.BankItems.Where(bi => !bi.IsClaimed).Select(bi => new BankItem(bi)))
                character.BankItems.TryAdd(bankItem.Slot, bankItem);

            if (dbCharacter.Guild != null)
            {
                character.GuildName = dbCharacter.Guild.Name;
                character.GuildRank = dbCharacter.GuildRank;
                character.GuildMembers.AddRange(dbCharacter.Guild.Members);
            }

            character.Init();

            return character;
        }

        /// <summary>
        ///  TODO: maybe it's better to have db procedure for this?
        ///  For now, we will clear old values, when character is loaded.
        /// </summary>
        public static void ClearOutdatedValues(IDatabase database, int characterId)
        {
            // Clear outdated buffs
            var outdatedBuffs = database.ActiveBuffs.Where(b => b.CharacterId == characterId && b.ResetTime < DateTime.UtcNow.AddSeconds(30));
            database.ActiveBuffs.RemoveRange(outdatedBuffs);

            // Clear expired items
            var expiredItems = database.CharacterItems.Where(i => i.CharacterId == characterId && i.ExpirationTime < DateTime.UtcNow.AddSeconds(30));
            database.CharacterItems.RemoveRange(expiredItems);

            database.SaveChanges();
        }
    }
}
