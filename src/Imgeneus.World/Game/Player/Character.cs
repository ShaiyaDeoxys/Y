using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.DatabaseBackgroundService;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character : BaseKillable, IKiller, IMapMember, IDisposable
    {
        private readonly ILogger<Character> _logger;
        private readonly IGameWorld _gameWorld;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IMapsLoader _mapLoader;
        private readonly PacketsHelper _packetsHelper;
        private readonly IGuildManager _guildManager;
        private readonly IGamePacketFactory _packetFactory;

        public IAdditionalInfoManager AdditionalInfoManager { get; private set; }
        public IInventoryManager InventoryManager { get; private set; }
        public IStealthManager StealthManager { get; private set; }
        public ILevelingManager LevelingManager { get; private set; }
        public ISpeedManager SpeedManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public ISkillsManager SkillsManager { get; private set; }
        public IKillsManager KillsManager { get; private set; }
        public IVehicleManager VehicleManager { get; private set; }
        public IShapeManager ShapeManager { get; private set; }
        public ILinkingManager LinkingManager { get; private set; }
        public ITeleportationManager TeleportationManager { get; private set; }
        public IPartyManager PartyManager { get; private set; }
        public ITradeManager TradeManager { get; private set; }
        public IFriendsManager FriendsManager { get; private set; }
        public IDuelManager DuelManager { get; private set; }
        public IGameSession GameSession { get; private set; }

        public Character(ILogger<Character> logger,
                         IGameWorld gameWorld,
                         IBackgroundTaskQueue taskQueue,
                         IDatabasePreloader databasePreloader,
                         IMapsLoader mapLoader,
                         IGuildManager guildManager,
                         ICountryProvider countryProvider,
                         ISpeedManager speedManager,
                         IStatsManager statsManager,
                         IAdditionalInfoManager additionalInfoManager,
                         IHealthManager healthManager,
                         ILevelProvider levelProvider,
                         ILevelingManager levelingManager,
                         IInventoryManager inventoryManager,
                         IStealthManager stealthManager,
                         IAttackManager attackManager,
                         ISkillsManager skillsManager,
                         IBuffsManager buffsManager,
                         IElementProvider elementProvider,
                         IKillsManager killsManager,
                         IVehicleManager vehicleManager,
                         IShapeManager shapeManager,
                         IMovementManager movementManager,
                         ILinkingManager linkinManager,
                         IMapProvider mapProvider,
                         ITeleportationManager teleportationManager,
                         IPartyManager partyManager,
                         ITradeManager tradeManager,
                         IFriendsManager friendsManager,
                         IDuelManager duelManager,
                         IGameSession gameSession,
                         IGamePacketFactory packetFactory) : base(databasePreloader, countryProvider, statsManager, healthManager, levelProvider, buffsManager, elementProvider, movementManager, mapProvider)
        {
            _logger = logger;
            _gameWorld = gameWorld;
            _taskQueue = taskQueue;
            _mapLoader = mapLoader;
            _guildManager = guildManager;
            _packetFactory = packetFactory;

            AdditionalInfoManager = additionalInfoManager;
            InventoryManager = inventoryManager;
            StealthManager = stealthManager;
            LevelingManager = levelingManager;
            SpeedManager = speedManager;
            AttackManager = attackManager;
            SkillsManager = skillsManager;
            KillsManager = killsManager;
            VehicleManager = vehicleManager;
            ShapeManager = shapeManager;
            LinkingManager = linkinManager;
            TeleportationManager = teleportationManager;
            PartyManager = partyManager;
            TradeManager = tradeManager;
            FriendsManager = friendsManager;
            DuelManager = duelManager;
            GameSession = gameSession;

            StatsManager.OnAdditionalStatsUpdate += SendAdditionalStats;
            StatsManager.OnResetStats += SendResetStats;
            BuffsManager.OnBuffAdded += OnBuffAdded;
            BuffsManager.OnBuffRemoved += OnBuffRemoved;
            AttackManager.OnStartAttack += SendAttackStart;
            VehicleManager.OnUsedVehicle += SendUseVehicle;
            SkillsManager.OnResetSkills += SendResetSkills;
            InventoryManager.OnAddItem += SendAddItemToInventory;
            InventoryManager.OnRemoveItem += SendRemoveItemFromInventory;
            InventoryManager.OnItemExpired += SendItemExpired;
            AttackManager.TargetOnBuffAdded += SendTargetAddBuff;
            AttackManager.TargetOnBuffRemoved += SendTargetRemoveBuff;
            DuelManager.OnDuelResponse += SendDuelResponse;
            DuelManager.OnStart += SendDuelStart;
            DuelManager.OnCanceled += SendDuelCancel;
            DuelManager.OnFinish += SendDuelFinish;
            LevelingManager.OnExpChanged += SendExperienceGain;

            _packetsHelper = new PacketsHelper();

            Bless.Instance.OnDarkBlessChanged += OnDarkBlessChanged;
            Bless.Instance.OnLightBlessChanged += OnLightBlessChanged;
        }

        private void Init()
        {
            InitQuests();
        }

        public void Dispose()
        {
            StatsManager.OnAdditionalStatsUpdate -= SendAdditionalStats;
            StatsManager.OnResetStats -= SendResetStats;
            BuffsManager.OnBuffAdded -= OnBuffAdded;
            BuffsManager.OnBuffRemoved -= OnBuffRemoved;
            AttackManager.OnStartAttack -= SendAttackStart;
            VehicleManager.OnUsedVehicle -= SendUseVehicle;
            SkillsManager.OnResetSkills -= SendResetSkills;
            InventoryManager.OnAddItem -= SendAddItemToInventory;
            InventoryManager.OnRemoveItem -= SendRemoveItemFromInventory;
            InventoryManager.OnItemExpired -= SendItemExpired;
            AttackManager.TargetOnBuffAdded -= SendTargetAddBuff;
            AttackManager.TargetOnBuffRemoved -= SendTargetRemoveBuff;
            DuelManager.OnDuelResponse -= SendDuelResponse;
            DuelManager.OnStart -= SendDuelStart;
            DuelManager.OnCanceled -= SendDuelCancel;
            DuelManager.OnFinish -= SendDuelFinish;
            LevelingManager.OnExpChanged -= SendExperienceGain;

            Bless.Instance.OnDarkBlessChanged -= OnDarkBlessChanged;
            Bless.Instance.OnLightBlessChanged -= OnLightBlessChanged;

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

        /// <summary>
        /// Creates character from database information.
        /// </summary>
        public static Character FromDbCharacter(DbCharacter dbCharacter, ILogger<Character> logger, IGameWorld gameWorld, IBackgroundTaskQueue taskQueue, IDatabasePreloader databasePreloader, IMapsLoader mapsLoader, ICountryProvider countryProvider, ISpeedManager speedManager, IStatsManager statsManager, IAdditionalInfoManager additionalInfoManager, IHealthManager healthManager, ILevelProvider levelProvider, ILevelingManager levelingManager, IInventoryManager inventoryManager, ILinkingManager linkingManager, IGuildManager guildManger, IStealthManager stealthManager, IAttackManager attackManager, ISkillsManager skillsManager, IBuffsManager buffsManager, IElementProvider elementProvider, IKillsManager killsManager, IVehicleManager vehicleManager, IShapeManager shapeManager, IMovementManager movementManager, IMapProvider mapProvider, ITeleportationManager teleportationManager, IPartyManager partyManager, ITradeManager tradeManager, IFriendsManager friendsManager, IDuelManager duelManager, IGameSession gameSession, IGamePacketFactory packetFactory)
        {
            var character = new Character(logger, gameWorld, taskQueue, databasePreloader, mapsLoader, guildManger, countryProvider, speedManager, statsManager, additionalInfoManager, healthManager, levelProvider, levelingManager, inventoryManager, stealthManager, attackManager, skillsManager, buffsManager, elementProvider, killsManager, vehicleManager, shapeManager, movementManager, linkingManager, mapProvider, teleportationManager, partyManager, tradeManager, friendsManager, duelManager, gameSession, packetFactory)
            {
                Id = dbCharacter.Id,
                Name = dbCharacter.Name,
                Points = dbCharacter.User.Points,
                GuildId = dbCharacter.GuildId
            };

            var quests = dbCharacter.Quests.Select(q => new Quest(databasePreloader, q)).ToList();
            character.Quests.AddRange(quests);

            character.QuickItems = dbCharacter.QuickItems;

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
