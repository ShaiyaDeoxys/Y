using Imgeneus.Database;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Blessing;
using Imgeneus.Game.Market;
using Imgeneus.Game.Recover;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Chat;
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
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Shop;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.Zone;
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
        public IGuildManager GuildManager { get; private set; }
        public IBankManager BankManager { get; private set; }
        public IQuestsManager QuestsManager { get; private set; }
        public IWarehouseManager WarehouseManager { get; private set; }
        public IShopManager ShopManager { get; private set; }
        public ISkillCastingManager SkillCastingManager { get; private set; }
        public ICastProtectionManager CastProtectionManager { get; private set; }
        public IBlessManager BlessManager { get; private set; }
        public IRecoverManager RecoverManager { get; private set; }
        public IMarketManager MarketManager { get; private set; }
        public IChatManager ChatManager { get; private set; }
        public IGameSession GameSession { get; private set; }

        public Character(ILogger<Character> logger,
                         IDatabasePreloader databasePreloader,
                         IGameDefinitionsPreloder definitionsPreloader,
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
                         IBankManager bankManager,
                         IQuestsManager questsManager,
                         IUntouchableManager untouchableManager,
                         IWarehouseManager warehouseManager,
                         IShopManager shopManager,
                         ISkillCastingManager skillCastingManager,
                         ICastProtectionManager castProtectionManager,
                         IBlessManager blessManager,
                         IRecoverManager recoverManager,
                         IMarketManager marketManager,
                         IChatManager chatManager,
                         IGameSession gameSession,
                         IGamePacketFactory packetFactory) : base(databasePreloader, definitionsPreloader, countryProvider, statsManager, healthManager, levelProvider, buffsManager, elementProvider, movementManager, untouchableManager, mapProvider)
        {
            _logger = logger;
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
            GuildManager = guildManager;
            BankManager = bankManager;
            QuestsManager = questsManager;
            WarehouseManager = warehouseManager;
            ShopManager = shopManager;
            SkillCastingManager = skillCastingManager;
            CastProtectionManager = castProtectionManager;
            BlessManager = blessManager;
            RecoverManager = recoverManager;
            MarketManager = marketManager;
            ChatManager = chatManager;
            GameSession = gameSession;

            HealthManager.MP_SP_Used += SendUseMPSP;
            HealthManager.OnCurrentHitpointsChanged += SendCurrentHitpoints;
            HealthManager.OnDead += IncreaseBless;
            StatsManager.OnAdditionalStatsUpdate += SendAdditionalStats;
            StatsManager.OnResetStats += SendResetStats;
            BuffsManager.OnBuffAdded += SendAddBuff;
            BuffsManager.OnBuffRemoved += SendRemoveBuff;
            //AttackManager.OnStartAttack += SendAutoAttackStop;
            VehicleManager.OnUsedVehicle += SendUseVehicle;
            SkillsManager.OnResetSkills += SendResetSkills;
            InventoryManager.OnAddItem += SendAddItemToInventory;
            InventoryManager.OnRemoveItem += SendRemoveItemFromInventory;
            InventoryManager.OnItemExpired += SendItemExpired;
            AttackManager.TargetOnBuffAdded += SendTargetAddBuff;
            AttackManager.TargetOnBuffRemoved += SendTargetRemoveBuff;
            AttackManager.OnTargetChanged += OnTargetChanged;
            DuelManager.OnDuelResponse += SendDuelResponse;
            DuelManager.OnStart += SendDuelStart;
            DuelManager.OnCanceled += SendDuelCancel;
            DuelManager.OnFinish += SendDuelFinish;
            LevelingManager.OnExpChanged += SendExperienceGain;
            QuestsManager.OnQuestMobCountChanged += SendQuestCountUpdate;
            QuestsManager.OnQuestFinished += SendQuestFinished;
            ShopManager.OnUseShopClosed += SendUseShopClosed;
            ShopManager.OnUseShopItemCountChanged += SendUseShopItemCountChanged;
            ShopManager.OnSoldItem += SendSoldItem;
            KillsManager.OnCountChanged += SendKillCountChanged;

            AddBlessBonuses(new BlessArgs(0, CountryProvider.Country == CountryType.Light ? BlessManager.LightAmount : BlessManager.DarkAmount));
            BlessManager.OnDarkBlessChanged += OnDarkBlessChanged;
            BlessManager.OnLightBlessChanged += OnLightBlessChanged;
        }

        private IKillable _target;
        private void OnTargetChanged(IKillable value)
        {
            if (_target == value)
                return;

            if (_target is not null)
            {
                _target.HealthManager.OnRecover -= SendMobTargetHP;
                _target.HealthManager.OnMaxHPChanged -= SendCharacterTargetMaxHP;
            }

            _target = value;

#if DEBUG
            if (_target is null)
                _logger.LogDebug("Target cleared.");
            else
                _logger.LogDebug("Target is {target} with id {id}", _target, _target.Id);
#endif

            if (_target is Mob)
            {
                _target.HealthManager.OnRecover += SendMobTargetHP;
            }

            if (_target is Character)
            {
                // Map cell will send recover for all players.
                //_target.HealthManager.OnRecover += SendCharacterTargetHP;
                _target.HealthManager.OnMaxHPChanged += SendCharacterTargetMaxHP;
            }
        }

        public void Dispose()
        {
            HealthManager.MP_SP_Used -= SendUseMPSP;
            HealthManager.OnCurrentHitpointsChanged -= SendCurrentHitpoints;
            HealthManager.OnDead -= IncreaseBless;
            StatsManager.OnAdditionalStatsUpdate -= SendAdditionalStats;
            StatsManager.OnResetStats -= SendResetStats;
            BuffsManager.OnBuffAdded -= SendAddBuff;
            BuffsManager.OnBuffRemoved -= SendRemoveBuff;
            //AttackManager.OnStartAttack -= SendAutoAttackStop;
            VehicleManager.OnUsedVehicle -= SendUseVehicle;
            SkillsManager.OnResetSkills -= SendResetSkills;
            InventoryManager.OnAddItem -= SendAddItemToInventory;
            InventoryManager.OnRemoveItem -= SendRemoveItemFromInventory;
            InventoryManager.OnItemExpired -= SendItemExpired;
            AttackManager.TargetOnBuffAdded -= SendTargetAddBuff;
            AttackManager.TargetOnBuffRemoved -= SendTargetRemoveBuff;
            AttackManager.OnTargetChanged -= OnTargetChanged;
            DuelManager.OnDuelResponse -= SendDuelResponse;
            DuelManager.OnStart -= SendDuelStart;
            DuelManager.OnCanceled -= SendDuelCancel;
            DuelManager.OnFinish -= SendDuelFinish;
            LevelingManager.OnExpChanged -= SendExperienceGain;
            QuestsManager.OnQuestMobCountChanged -= SendQuestCountUpdate;
            QuestsManager.OnQuestFinished -= SendQuestFinished;
            ShopManager.OnUseShopClosed -= SendUseShopClosed;
            ShopManager.OnUseShopItemCountChanged -= SendUseShopItemCountChanged;
            ShopManager.OnSoldItem -= SendSoldItem;
            KillsManager.OnCountChanged -= SendKillCountChanged;

            BlessManager.OnDarkBlessChanged -= OnDarkBlessChanged;
            BlessManager.OnLightBlessChanged -= OnLightBlessChanged;

            Map = null;
        }

#region Quick skill bar

        /// <summary>
        /// Quick items, i.e. skill bars. Not sure if I need to store it as DbQuickSkillBarItem or need another connector helper class here?
        /// </summary>
        public IEnumerable<DbQuickSkillBarItem> QuickItems;

#endregion

        /// <summary>
        /// Creates character from database information.
        /// </summary>
        public static Character FromDbCharacter(
            DbCharacter dbCharacter,
            ILogger<Character> logger,
            IDatabasePreloader databasePreloader,
            IGameDefinitionsPreloder definitionsPreloader,
            ICountryProvider countryProvider,
            ISpeedManager speedManager,
            IStatsManager statsManager,
            IAdditionalInfoManager additionalInfoManager,
            IHealthManager healthManager,
            ILevelProvider levelProvider,
            ILevelingManager levelingManager,
            IInventoryManager inventoryManager,
            ILinkingManager linkingManager,
            IGuildManager guildManger,
            IStealthManager stealthManager,
            IAttackManager attackManager,
            ISkillsManager skillsManager,
            IBuffsManager buffsManager,
            IElementProvider elementProvider,
            IKillsManager killsManager,
            IVehicleManager vehicleManager,
            IShapeManager shapeManager,
            IMovementManager movementManager,
            IMapProvider mapProvider,
            ITeleportationManager teleportationManager,
            IPartyManager partyManager,
            ITradeManager tradeManager,
            IFriendsManager friendsManager,
            IDuelManager duelManager,
            IBankManager bankManager,
            IQuestsManager questsManager,
            IUntouchableManager untouchableManager,
            IWarehouseManager warehouseManager,
            IShopManager shopManager,
            ISkillCastingManager skillCastingManager,
            ICastProtectionManager castProtectionManager,
            IBlessManager blessManager,
            IRecoverManager recoverManager,
            IMarketManager marketManager,
            IChatManager chatManager,
            IGameSession gameSession,
            IGamePacketFactory packetFactory)
        {
            var character = new Character(logger,
                                          databasePreloader,
                                          definitionsPreloader,
                                          guildManger,
                                          countryProvider,
                                          speedManager,
                                          statsManager,
                                          additionalInfoManager,
                                          healthManager,
                                          levelProvider,
                                          levelingManager,
                                          inventoryManager,
                                          stealthManager,
                                          attackManager,
                                          skillsManager,
                                          buffsManager,
                                          elementProvider,
                                          killsManager,
                                          vehicleManager,
                                          shapeManager,
                                          movementManager,
                                          linkingManager,
                                          mapProvider,
                                          teleportationManager,
                                          partyManager,
                                          tradeManager,
                                          friendsManager,
                                          duelManager,
                                          bankManager,
                                          questsManager,
                                          untouchableManager,
                                          warehouseManager,
                                          shopManager,
                                          skillCastingManager,
                                          castProtectionManager,
                                          blessManager,
                                          recoverManager,
                                          marketManager,
                                          chatManager,
                                          gameSession,
                                          packetFactory)
            {
                Id = dbCharacter.Id
            };

            character.QuickItems = dbCharacter.QuickItems;

            return character;
        }

        /// <summary>
        ///  TODO: maybe it's better to have db procedure for this?
        ///  For now, we will clear old values, when character is loaded.
        /// </summary>
        public static void ClearOutdatedValues(IDatabase database, uint characterId)
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
