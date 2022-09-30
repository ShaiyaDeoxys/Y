using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Chat;
using Imgeneus.World.Game.Dyeing;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using Imgeneus.World.Game.Notice;
using Imgeneus.World.Game.Guild;
using Imgeneus.Database;
using Imgeneus.World.Game.Time;
using System.Collections.Concurrent;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Packets;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Stealth;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Vehicle;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Kills;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Trade;
using Imgeneus.World.Game.Friends;
using Imgeneus.World.Game.Duel;
using Imgeneus.World.Game.Bank;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using System.Threading;
using System.Threading.Tasks;
using Imgeneus.World.Game.Etin;
using Imgeneus.GameDefinitions;
using Parsec.Shaiya.NpcQuest;
using Imgeneus.World.Tests.NpcTests;
using Parsec.Shaiya.Svmap;
using Npc = Imgeneus.World.Game.NPCs.Npc;
using SQuest = Parsec.Shaiya.NpcQuest.Quest;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Game.Warehouse;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Shop;
using Parsec.Shaiya.Skill;
using Element = Imgeneus.Database.Constants.Element;
using Imgeneus.Game.Skills;
using Imgeneus.Game.Blessing;
using Imgeneus.Game.Recover;
using Imgeneus.Game.Monster;
using Imgeneus.Game.Market;
using Imgeneus.GameDefinitions.Constants;
using Imgeneus.GameDefinitions.Enums;
using Imgeneus.Logs;

namespace Imgeneus.World.Tests
{
    public abstract class BaseTest
    {
        protected Mock<IGameWorld> gameWorldMock = new Mock<IGameWorld>();
        protected Mock<IDatabasePreloader> databasePreloader = new Mock<IDatabasePreloader>();
        protected Mock<IGameDefinitionsPreloder> definitionsPreloader = new Mock<IGameDefinitionsPreloder>();
        protected Mock<IDatabase> databaseMock = new Mock<IDatabase>();
        protected Mock<ITimeService> timeMock = new Mock<ITimeService>();
        protected Mock<IMapsLoader> mapsLoaderMock = new Mock<IMapsLoader>();
        protected Mock<IGamePacketFactory> packetFactoryMock = new Mock<IGamePacketFactory>();
        protected Mock<ICharacterConfiguration> config = new Mock<ICharacterConfiguration>();
        protected Mock<ILogger<Map>> mapLoggerMock = new Mock<ILogger<Map>>();
        protected Mock<ILogger<Mob>> mobLoggerMock = new Mock<ILogger<Mob>>();
        protected Mock<ILogger<Npc>> npcLoggerMock = new Mock<ILogger<Npc>>();
        protected Mock<ILogger<IGuildManager>> guildLoggerMock = new Mock<ILogger<IGuildManager>>();
        protected Mock<IChatManager> chatMock = new Mock<IChatManager>();
        protected Mock<IDyeingManager> dyeingMock = new Mock<IDyeingManager>();
        protected Mock<IWorldClient> worldClientMock = new Mock<IWorldClient>();
        protected Mock<IMobFactory> mobFactoryMock = new Mock<IMobFactory>();
        protected Mock<INpcFactory> npcFactoryMock = new Mock<INpcFactory>();
        protected Mock<IObeliskFactory> obeliskFactoryMock = new Mock<IObeliskFactory>();
        protected Mock<INoticeManager> noticeManagerMock = new Mock<INoticeManager>();
        protected Mock<IGameSession> gameSessionMock = new Mock<IGameSession>();
        protected Mock<IEtinManager> etinMock = new Mock<IEtinManager>();
        protected Mock<IItemEnchantConfiguration> enchantConfig = new Mock<IItemEnchantConfiguration>();
        protected Mock<IItemCreateConfiguration> itemCreateConfig = new Mock<IItemCreateConfiguration>();
        protected Mock<ILogsManager> logsManagerMock = new Mock<ILogsManager>();
        protected BlessManager BlessManager = new BlessManager();

        protected Map testMap
        {
            get
            {
                var map = new Map(
                        Map.TEST_MAP_ID,
                        new MapDefinition(),
                        new Svmap() { MapSize = 100, CellSize = 100 },
                        new List<ObeliskConfiguration>(),
                        new List<BossConfiguration>(),
                        mapLoggerMock.Object,
                        packetFactoryMock.Object,
                        definitionsPreloader.Object,
                        mobFactoryMock.Object,
                        npcFactoryMock.Object,
                        obeliskFactoryMock.Object,
                        timeMock.Object);

                map.GameWorld = gameWorldMock.Object;

                return map;
            }
        }

        private uint _characterId;
        protected Character CreateCharacter(Map map = null, Fraction country = Fraction.Light, GuildConfiguration guildConfiguration = null, GuildHouseConfiguration guildHouseConfiguration = null, CharacterProfession profession = CharacterProfession.Fighter)
        {
            _characterId++;

            var countryProvider = new CountryProvider(new Mock<ILogger<CountryProvider>>().Object);
            countryProvider.Init(_characterId, country);

            var levelProvider = new LevelProvider(new Mock<ILogger<LevelProvider>>().Object);
            levelProvider.Init(_characterId, 1);

            var mapProvider = new MapProvider(new Mock<ILogger<MapProvider>>().Object);
            var stealthManager = new StealthManager(new Mock<ILogger<StealthManager>>().Object);
            var speedManager = new SpeedManager(new Mock<ILogger<SpeedManager>>().Object, stealthManager);
            var additionalInfoManager = new AdditionalInfoManager(new Mock<ILogger<AdditionalInfoManager>>().Object, config.Object, databaseMock.Object);
            additionalInfoManager.Init(_characterId, Race.Human, profession, 0, 0, 0, Gender.Man, Mode.Ultimate, 0, string.Empty);

            var statsManager = new StatsManager(new Mock<ILogger<StatsManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object);

            var healthManager = new HealthManager(new Mock<ILogger<HealthManager>>().Object, statsManager, levelProvider, config.Object, databaseMock.Object, additionalInfoManager);
            healthManager.Init(_characterId, 0, 0, 0, null, null, null);

            var elementProvider = new ElementProvider(new Mock<ILogger<ElementProvider>>().Object);
            var untouchableManager = new UntouchableManager(new Mock<ILogger<UntouchableManager>>().Object);
            var movementManager = new MovementManager(new Mock<ILogger<MovementManager>>().Object);
            movementManager.Init(_characterId, 0, 0, 0, 0, MoveMotion.Run);

            var partyManager = new PartyManager(new Mock<ILogger<PartyManager>>().Object, packetFactoryMock.Object, gameWorldMock.Object, mapProvider, healthManager, countryProvider, BlessManager);
            partyManager.Init(_characterId);

            var levelingManager = new LevelingManager(new Mock<ILogger<LevelingManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object, databasePreloader.Object, partyManager, mapProvider, movementManager);
            levelingManager.Init(_characterId, 0);

            var teleportManager = new TeleportationManager(new Mock<ILogger<TeleportationManager>>().Object, movementManager, mapProvider, databaseMock.Object, countryProvider, levelProvider, gameWorldMock.Object, healthManager, BlessManager);
            teleportManager.Init(_characterId, new List<DbCharacterSavePositions>());

            var vehicleManager = new VehicleManager(new Mock<ILogger<VehicleManager>>().Object, stealthManager, speedManager, healthManager, gameWorldMock.Object);
            var shapeManager = new ShapeManager(new Mock<ILogger<ShapeManager>>().Object, stealthManager, vehicleManager);
            shapeManager.Init(_characterId);

            var warehouseManager = new WarehouseManager(new Mock<ILogger<WarehouseManager>>().Object, databaseMock.Object, definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, gameWorldMock.Object, packetFactoryMock.Object);
            var attackManager = new AttackManager(new Mock<ILogger<AttackManager>>().Object, statsManager, levelProvider, elementProvider, countryProvider, speedManager, stealthManager, healthManager, shapeManager, movementManager, vehicleManager);
            attackManager.AlwaysHit = true;

            var castProtectionManager = new CastProtectionManager(new Mock<ILogger<CastProtectionManager>>().Object, movementManager, partyManager, packetFactoryMock.Object, new Mock<IGameSession>().Object, mapProvider);
            var recoverManager = new Mock<IRecoverManager>().Object;
            var buffsManager = new BuffsManager(new Mock<ILogger<BuffsManager>>().Object, databaseMock.Object, definitionsPreloader.Object, statsManager, healthManager, speedManager, elementProvider, untouchableManager, stealthManager, levelingManager, attackManager, teleportManager, warehouseManager, shapeManager, castProtectionManager, movementManager, additionalInfoManager, mapProvider, gameWorldMock.Object, recoverManager);
            buffsManager.Init(_characterId);

            var skillsManager = new SkillsManager(new Mock<ILogger<SkillsManager>>().Object, definitionsPreloader.Object, databaseMock.Object, healthManager, attackManager, buffsManager, statsManager, elementProvider, countryProvider, config.Object, levelProvider, additionalInfoManager, mapProvider, teleportManager, movementManager, shapeManager, speedManager, partyManager, packetFactoryMock.Object);
            skillsManager.Init(_characterId, new List<Skill>());
            var skillCastingManager = new SkillCastingManager(new Mock<ILogger<SkillCastingManager>>().Object, movementManager, teleportManager, healthManager, skillsManager, buffsManager, gameWorldMock.Object, castProtectionManager);
            skillCastingManager.Init(_characterId);
            var inventoryManager = new InventoryManager(new Mock<ILogger<InventoryManager>>().Object, definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, databaseMock.Object, statsManager, healthManager, speedManager, elementProvider, vehicleManager, levelProvider, levelingManager, countryProvider, gameWorldMock.Object, additionalInfoManager, skillsManager, buffsManager, config.Object, attackManager, partyManager, teleportManager, new Mock<IChatManager>().Object, warehouseManager, BlessManager, logsManagerMock.Object);
            inventoryManager.Init(_characterId, new List<DbCharacterItems>(), 0);


            var killsManager = new KillsManager(new Mock<ILogger<KillsManager>>().Object, databaseMock.Object, healthManager, countryProvider, mapProvider, movementManager, levelProvider, statsManager, inventoryManager);
            var guildManager = new GuildManager(new Mock<ILogger<GuildManager>>().Object, guildConfiguration, guildHouseConfiguration, gameWorldMock.Object, timeMock.Object, inventoryManager, partyManager, countryProvider, etinMock.Object, null);
            guildManager.Init(_characterId);

            var linkingManager = new LinkingManager(new Mock<ILogger<LinkingManager>>().Object, definitionsPreloader.Object, inventoryManager, statsManager, healthManager, speedManager, guildManager, mapProvider, enchantConfig.Object, itemCreateConfig.Object, countryProvider, BlessManager);
            var tradeManager = new TradeManager(new Mock<ILogger<TradeManager>>().Object, gameWorldMock.Object, inventoryManager);
            var friendsManager = new FriendsManager(new Mock<ILogger<FriendsManager>>().Object, databaseMock.Object, gameWorldMock.Object);
            var duelManager = new DuelManager(new Mock<ILogger<DuelManager>>().Object, gameWorldMock.Object, tradeManager, movementManager, healthManager, killsManager, mapProvider, inventoryManager, teleportManager);
            duelManager.Init(_characterId);

            var bankManager = new BankManager(new Mock<ILogger<BankManager>>().Object, databaseMock.Object, definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, inventoryManager);
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, mapProvider, gameWorldMock.Object, databaseMock.Object, partyManager, inventoryManager, enchantConfig.Object, itemCreateConfig.Object, levelingManager);
            var shopManager = new ShopManager(new Mock<ILogger<ShopManager>>().Object, inventoryManager, mapProvider);

            var character = new Character(
                new Mock<ILogger<Character>>().Object,
                databasePreloader.Object,
                definitionsPreloader.Object,
                guildManager,
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
                teleportManager,
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
                BlessManager,
                recoverManager,
                new Mock<IMarketManager>().Object,
                chatMock.Object,
                gameSessionMock.Object,
                packetFactoryMock.Object);


            character.Id = _characterId;
            character.HealthManager.IncreaseHP(1);

            if (map != null)
                map.LoadPlayer(character);

            gameWorldMock.Object.Players.TryAdd(character.Id, character);

            return character;
        }

        private uint _mobId;

        protected Mob CreateMob(ushort mobId, Map map, Fraction country = Fraction.NotSelected)
        {
            _mobId++;
            var dbMob = definitionsPreloader.Object.Mobs[mobId];

            var countryProvider = new CountryProvider(new Mock<ILogger<CountryProvider>>().Object);
            countryProvider.Init(0, country);

            var mapProvider = new MapProvider(new Mock<ILogger<MapProvider>>().Object);
            var levelProvider = new LevelProvider(new Mock<ILogger<LevelProvider>>().Object);
            levelProvider.Init(_mobId, dbMob.Level);

            var levelingManager = new Mock<ILevelingManager>();
            var additionalInfoManager = new AdditionalInfoManager(new Mock<ILogger<AdditionalInfoManager>>().Object, config.Object, databaseMock.Object);
            var statsManager = new StatsManager(new Mock<ILogger<StatsManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object);
            var healthManager = new HealthManager(new Mock<ILogger<HealthManager>>().Object, statsManager, levelProvider, config.Object, databaseMock.Object, null);
            healthManager.Init(_mobId, dbMob.HP, dbMob.MP, dbMob.SP, dbMob.HP, dbMob.MP, dbMob.SP);

            var stealthManager = new StealthManager(new Mock<ILogger<StealthManager>>().Object);
            var speedManager = new SpeedManager(new Mock<ILogger<SpeedManager>>().Object, stealthManager);
            var elementProvider = new ElementProvider(new Mock<ILogger<ElementProvider>>().Object);
            var untouchableManager = new UntouchableManager(new Mock<ILogger<UntouchableManager>>().Object);
            var movementManager = new MovementManager(new Mock<ILogger<MovementManager>>().Object);
            var attackManager = new AttackManager(new Mock<ILogger<AttackManager>>().Object, statsManager, levelProvider, elementProvider, countryProvider, speedManager, stealthManager, healthManager, new Mock<IShapeManager>().Object, movementManager, new Mock<IVehicleManager>().Object);
            attackManager.AlwaysHit = true;

            var buffsManager = new BuffsManager(new Mock<ILogger<BuffsManager>>().Object, databaseMock.Object, definitionsPreloader.Object, statsManager, healthManager, speedManager, elementProvider, untouchableManager, stealthManager, levelingManager.Object, attackManager, null, null, null, null, movementManager, null, mapProvider, gameWorldMock.Object, null);
            var skillsManager = new SkillsManager(new Mock<ILogger<SkillsManager>>().Object, definitionsPreloader.Object, databaseMock.Object, healthManager, attackManager, buffsManager, statsManager, elementProvider, countryProvider, config.Object, levelProvider, additionalInfoManager, mapProvider, null, movementManager, new Mock<IShapeManager>().Object, speedManager, null, packetFactoryMock.Object);
            var aiManager = new AIManager(new Mock<ILogger<AIManager>>().Object, movementManager, countryProvider, attackManager, untouchableManager, mapProvider, skillsManager, statsManager, elementProvider, definitionsPreloader.Object, speedManager, healthManager, buffsManager);

            var mob = new Mob(
                mobId,
                true,
                new MoveArea(0, 0, 0, 0, 0, 0),
                mobLoggerMock.Object,
                databasePreloader.Object,
                definitionsPreloader.Object,
                aiManager,
                enchantConfig.Object,
                itemCreateConfig.Object,
                countryProvider,
                statsManager,
                healthManager,
                levelProvider,
                speedManager,
                attackManager,
                skillsManager,
                buffsManager,
                elementProvider,
                movementManager,
                untouchableManager,
                mapProvider);

            if (map != null)
                map.AddMob(mob);

            return mob;
        }

        public BaseTest()
        {
            gameWorldMock.Setup(x => x.Players)
                .Returns(new ConcurrentDictionary<uint, Character>());

            PortalTeleportNotAllowedReason reason;
            gameWorldMock.Setup(x => x.CanTeleport(It.IsAny<Character>(), It.IsAny<ushort>(), out reason, false))
                .Returns(true);

            config.Setup((conf) => conf.GetConfig(It.IsAny<int>()))
                  .Returns(new Character_HP_SP_MP() { HP = 100, MP = 200, SP = 300 });

            config.Setup((conf) => conf.DefaultStats)
                  .Returns(new DefaultStat[1] {
                      new DefaultStat()
                      {
                          Job = CharacterProfession.Fighter,
                          Str = 12,
                          Dex = 11,
                          Rec = 10,
                          Int = 8,
                          Wis = 9,
                          Luc = 10,
                          MainStat = 0
                      }
                  });

            config.Setup((conf) => conf.GetMaxLevelConfig(It.IsAny<Mode>()))
                .Returns(
                    new DefaultMaxLevel()
                    {
                        Mode = Mode.Ultimate,
                        Level = 80
                    }
                );

            config.Setup((conf) => conf.GetLevelStatSkillPoints(It.IsAny<Mode>()))
                .Returns(
                    new DefaultLevelStatSkillPoints()
                    {
                        Mode = Mode.Ultimate,
                        StatPoint = 9,
                        SkillPoint = 7
                    }
                );

            enchantConfig.Setup((conf) => conf.LapisianEnchantAddValue)
                .Returns(
                new Dictionary<string, int>()
                {
                    { "WeaponStep00", 0 },
                    { "WeaponStep01", 7 },
                    { "WeaponStep19", 286 },
                    { "WeaponStep20", 311 },
                    { "DefenseStep00", 0 },
                    { "DefenseStep01", 5 },
                    { "DefenseStep18", 90 },
                    { "DefenseStep19", 95 },
                    { "DefenseStep20", 100 }
                });

            enchantConfig.Setup((conf) => conf.LapisianEnchantPercentRate)
                .Returns(
                new Dictionary<string, int>()
                {
                    { "WeaponStep00", 900000 },
                    { "WeaponStep01", 800000 },
                    { "WeaponStep19", 200 },
                    { "WeaponStep20", 0 },
                    { "DefenseStep00", 990000 },
                    { "DefenseStep01", 980000 },
                    { "DefenseStep19", 200 },
                    { "DefenseStep20", 0 }
                });

            databasePreloader
                .SetupGet((preloader) => preloader.Levels)
                .Returns(new Dictionary<(Mode mode, ushort level), DbLevel>()
                {
                    { (Mode.Beginner, 1), Level1_Mode1 },
                    { (Mode.Normal, 1), Level1_Mode2 },
                    { (Mode.Hard, 1), Level1_Mode3 },
                    { (Mode.Ultimate, 1), Level1_Mode4 },
                    { (Mode.Beginner, 2), Level2_Mode1 },
                    { (Mode.Normal, 2), Level2_Mode2 },
                    { (Mode.Hard, 2), Level2_Mode3 },
                    { (Mode.Ultimate, 2), Level2_Mode4 },
                    { (Mode.Beginner, 37), Level37_Mode1 },
                    { (Mode.Normal, 37), Level37_Mode2 },
                    { (Mode.Hard, 37), Level37_Mode3 },
                    { (Mode.Ultimate, 37), Level37_Mode4 },
                    { (Mode.Beginner, 38), Level38_Mode1 },
                    { (Mode.Normal, 38), Level38_Mode2 },
                    { (Mode.Hard, 38), Level38_Mode3 },
                    { (Mode.Ultimate, 38), Level38_Mode4 },
                    { (Mode.Beginner, 79), Level79_Mode1 },
                    { (Mode.Normal, 79), Level79_Mode2 },
                    { (Mode.Hard, 79), Level79_Mode3 },
                    { (Mode.Ultimate, 79), Level79_Mode4 },
                    { (Mode.Beginner, 80), Level80_Mode1 },
                    { (Mode.Normal, 80), Level80_Mode2 },
                    { (Mode.Hard, 80), Level80_Mode3 },
                    { (Mode.Ultimate, 80), Level80_Mode4 },
                });

            definitionsPreloader
                .SetupGet((preloader) => preloader.Mobs)
                .Returns(new Dictionary<ushort, DbMob>()
                {
                    { 1, Wolf },
                    { 3041, CrypticImmortal }
                });

            definitionsPreloader
                .SetupGet((preloader) => preloader.MobItems)
                .Returns(new Dictionary<(ushort MobId, byte ItemOrder), DbMobItems>());

            definitionsPreloader
                .SetupGet((preloader) => preloader.NPCs)
                .Returns(new Dictionary<(NpcType Type, short TypeId), BaseNpc>()
                {
                    { (NpcType.Merchant, 1), WeaponMerchant }
                });


            definitionsPreloader
                .SetupGet((preloader) => preloader.Items)
                .Returns(new Dictionary<(byte Type, byte TypeId), DbItem>()
                {
                    { (17, 2), WaterArmor },
                    { (2, 92), FireSword },
                    { (6, 1), Spear },
                    { (100, 192), PerfectLinkingHammer },
                    { (44, 237), PerfectExtractingHammer },
                    { (100, 139), LuckyCharm },
                    { (17, 59), JustiaArmor },
                    { (30, 1), Gem_Str_Level_1 },
                    { (30, 2), Gem_Str_Level_2 },
                    { (30, 3), Gem_Str_Level_3 },
                    { (30, 7), Gem_Str_Level_7 },
                    { (100, 1), EtainPotion },
                    { (25, 1), RedApple },
                    { (25, 13), GreenApple },
                    { (42, 1), HorseSummonStone },
                    { (42, 136), Nimbus1d },
                    { (100, 95), Item_HealthRemedy_Level_1  },
                    { (101, 71), Item_AbsorbRemedy },
                    { (30, 240), Gem_Absorption_Level_4 },
                    { (30, 241), Gem_Absorption_Level_5 },
                    { (43, 3), Etin_100 },
                    { (100, 107), SpeedyRemedy },
                    { (100, 45), PartySummonRune },
                    { (100, 108), MinSunExpStone },
                    { (95, 1), AssaultLapisia },
                    { (95, 6), ProtectorsLapisia },
                    { (95, 22), PerfectWeaponLapisia_Lvl1 },
                    { (95, 23), PerfectWeaponLapisia_Lvl2 },
                    { (95, 42), PerfectArmorLapisia_Lvl1 },
                    { (95, 8), LapisiaBreakItem },
                    { (100, 65), TeleportationStone },
                    { (100, 72), BlueDragonCharm },
                    { (100, 78), BoxWithApples },
                    { (100, 75), DoubleWarehouse },
                });

            definitionsPreloader
                .SetupGet((preloader) => preloader.ItemsByGrade)
                .Returns(new Dictionary<ushort, List<DbItem>>() {
                    { 1, new List<DbItem>() { RedApple, GreenApple } }
                });

            NewBeginnings.Results.Add(new QuestResult() { Exp = 5, Money = 3000 });

            Bartering.FarmItems.Add(new QuestItem() { Type = RedApple.Type, TypeId = RedApple.TypeId, Count = 10 });
            Bartering.Results.Add(new QuestResult() { Exp = 3, Money = 3000, ItemType1 = RedApple.Type, ItemTypeId1 = RedApple.TypeId, ItemCount1 = 20 });

            SkillsAndStats.Results.Add(new QuestResult() { ItemType1 = WaterArmor.Type, ItemTypeId1 = WaterArmor.TypeId, ItemCount1 = 1 });
            SkillsAndStats.Results.Add(new QuestResult() { ItemType1 = FireSword.Type, ItemTypeId1 = FireSword.TypeId, ItemCount1 = 1 });

            definitionsPreloader
                .SetupGet((preloader) => preloader.Quests)
                .Returns(new Dictionary<short, SQuest>()
                {
                    { NewBeginnings.Id, NewBeginnings },
                    { Bartering.Id, Bartering },
                    { SkillsAndStats.Id, SkillsAndStats }
                });

            definitionsPreloader
                .SetupGet((preloader) => preloader.Skills)
                .Returns(new Dictionary<(ushort SkillId, byte SkillLevel), DbSkill>()
                {
                    { (1, 1) , StrengthTraining },
                    { (14, 1), ManaTraining },
                    { (15, 1), SharpenWeaponMastery_Lvl1 },
                    { (15, 2), SharpenWeaponMastery_Lvl2 },
                    { (35, 1), MagicRoots_Lvl1 },
                    { (273, 100), AttributeRemove },
                    { (732, 1), FireWeapon },
                    { (735, 1), EarthWeapon },
                    { (762, 1), FireSkin },
                    { (765, 1), EarthSkin },
                    { (672, 1), Panic_Lvl1 },
                    { (787, 1), Dispel },
                    { (256, 1), Skill_HealthRemedy_Level1 },
                    { (112, 1), Leadership },
                    { (222, 1), EXP },
                    { (0, 1) , skill1_level1 },
                    { (0, 2) , skill1_level2 },
                    { (418, 11), BlastAbsorbRedemySkill },
                    { (655, 1), Untouchable },
                    { (724, 1), BullsEye },
                    { (63, 1), Stealth },
                    { (249, 1), SpeedRemedy_Lvl1 },
                    { (250, 1), MinSunStone_Lvl1 },
                    { (231, 1), BlueDragonCharm_Lvl1 },
                    { (234, 1), DoubleWarehouseStone_Lvl1 },
                    { (613, 1), MainWeaponPowerUp },
                    { (711, 10), FleetFoot },
                    { (460, 1), Transformation },
                    { (623, 1), BerserkersRage },
                    { (630, 1), WildRage },
                    { (627, 1), DeadlyStrike },
                    { (340, 1), NettleSting },
                    { (631, 1), Eraser },
                    { (636, 1), BloodyArc },
                    { (615, 1), IntervalTraining },
                    { (772, 1), MagicVeil },
                    { (775, 1), EtainsEmbrace },
                    { (776, 1), MagicMirror },
                    { (779, 1), PersistBarrier },
                    { (785, 1), Healing },
                    { (364, 1), FrostBarrier },
                    { (792, 1), Resurrection },
                    { (777, 1), Hypnosis },
                    { (793, 1), Evolution },
                    { (794, 1), Polymorph },
                    { (791, 1), Detection },
                    { (786, 1), HealingPrayer },
                    { (677, 1), TransformationAssassin },
                    { (680, 1), Disguise },
                    { (702, 1), Misfortune },
                    { (693, 1), StunSlam },
                    { (701, 1), DeathTouch },
                    { (692, 1), PhantomAssault },
                    { (696, 1), DisruptionStun },
                    { (717, 1), PotentialForce },
                    { (742, 1), Diversion },
                    { (741, 2), UltimateBarrier },
                    { (710, 1), LongRange },
                    { (661, 1), HealthDrain },
                    { (664, 1), SilencingBlow },
                    { (665, 1), BlindingBlow },
                    { (305, 1), Provoke }
                });

            databaseMock
                .Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(1));

            itemCreateConfig.Setup(x => x.ItemCreateInfo)
                .Returns(new Dictionary<ushort, IEnumerable<ItemCreateInfo>>()
                {
                    { 1, new List<ItemCreateInfo>() { new ItemCreateInfo() { Grade = 1, Weight = 1 } } }
                });
        }

        #region Test mobs

        protected DbMob Wolf = new DbMob()
        {
            Id = 1,
            AI = MobAI.Combative,
            Level = 38,
            HP = 2765,
            Element = Element.Wind1,
            AttackSpecial3 = MobRespawnTime.Seconds_15,
            Exp = 70,
            ChaseStep = 9,
            ChaseRange = 9,
            ChaseTime = 1800,
            NormalStep = 6,
            NormalTime = 4000,
            ResistState15 = 1
        };

        protected DbMob CrypticImmortal = new DbMob()
        {
            Id = 3041,
            AI = MobAI.CrypticImmortal,
            Level = 75,
            HP = 35350000,
            AttackOk1 = 1,
            Attack1 = 8822,
            AttackPlus1 = 3222,
            AttackRange1 = 5,
            AttackTime1 = 2500,
            NormalTime = 1,
            ChaseTime = 1,
            Exp = 3253
        };

        #endregion

        #region Skills

        protected DbSkill StrengthTraining = new DbSkill()
        {
            SkillId = 1,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PassiveDefence,
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.PhysicalAttackPower,
            AbilityValue1 = 18,
            SkillPoint = 1
        };

        protected DbSkill ManaTraining = new DbSkill()
        {
            SkillId = 14,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PassiveDefence,
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.MP,
            AbilityValue1 = 110
        };

        protected DbSkill SharpenWeaponMastery_Lvl1 = new DbSkill()
        {
            SkillId = 15,
            SkillLevel = 1,
            TypeDetail = TypeDetail.WeaponMastery,
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 3,
            Weaponvalue = 1
        };

        protected DbSkill SharpenWeaponMastery_Lvl2 = new DbSkill()
        {
            SkillId = 15,
            SkillLevel = 2,
            TypeDetail = TypeDetail.WeaponMastery,
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 3,
            Weaponvalue = 2
        };

        protected DbSkill MagicRoots_Lvl1 = new DbSkill()
        {
            SkillId = 35,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Immobilize,
            DamageHP = 42,
            TypeAttack = TypeAttack.MagicAttack,
            ResetTime = 10,
            KeepTime = 5,
            DamageType = DamageType.PlusExtraDamage,
            TargetType = TargetType.SelectedEnemy,
            ReadyTime = 5,
            TypeEffect = TypeEffect.Debuff,
            AttackRange = 30
        };

        protected DbSkill AttributeRemove = new DbSkill()
        {
            SkillId = 273,
            SkillLevel = 100,
            TypeDetail = TypeDetail.RemoveAttribute,
            TypeAttack = TypeAttack.MagicAttack,
            DamageType = DamageType.FixedDamage,
            KeepTime = 3600
        };

        protected DbSkill FireWeapon = new DbSkill()
        {
            SkillId = 732,
            SkillLevel = 1,
            TypeDetail = TypeDetail.ElementalAttack,
            Element = Element.Fire1,
            TypeAttack = TypeAttack.ShootingAttack,
            KeepTime = 60
        };

        protected DbSkill EarthWeapon = new DbSkill()
        {
            SkillId = 735,
            SkillLevel = 1,
            TypeDetail = TypeDetail.ElementalAttack,
            Element = Element.Earth1,
            TypeAttack = TypeAttack.ShootingAttack,
            KeepTime = 60
        };

        protected DbSkill FireSkin = new DbSkill()
        {
            SkillId = 762,
            SkillLevel = 1,
            TypeDetail = TypeDetail.ElementalProtection,
            Element = Element.Fire1,
            TypeAttack = TypeAttack.MagicAttack,
            KeepTime = 60
        };

        protected DbSkill EarthSkin = new DbSkill()
        {
            SkillId = 765,
            SkillLevel = 1,
            TypeDetail = TypeDetail.ElementalProtection,
            Element = Element.Earth1,
            TypeAttack = TypeAttack.MagicAttack,
            KeepTime = 60
        };

        protected DbSkill Panic_Lvl1 = new DbSkill()
        {
            SkillId = 672,
            SkillLevel = 1,
            TypeDetail = TypeDetail.SubtractingDebuff,
            AbilityType1 = AbilityType.PhysicalDefense,
            AbilityValue1 = 119,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Debuff,
            KeepTime = 6
        };

        protected DbSkill Dispel = new DbSkill()
        {
            SkillId = 787,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Dispel,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.HealingDispel
        };

        protected DbSkill Skill_HealthRemedy_Level1 = new DbSkill()
        {
            SkillId = 256,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.HP,
            AbilityValue1 = 500,
            FixRange = Duration.DurationInMinutes,
            KeepTime = 3600

        };

        protected DbSkill Leadership = new DbSkill()
        {
            SkillId = 112,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.AlliesNearCaster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            ApplyRange = 50,
            AbilityType1 = AbilityType.PhysicalAttackPower,
            AbilityValue1 = 13,
            FixRange = Duration.ClearAfterDeath,
            KeepTime = 900
        };

        protected DbSkill EXP = new DbSkill()
        {
            SkillId = 222,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            ApplyRange = 50,
            AbilityType1 = AbilityType.ExpGainRate,
            AbilityValue1 = 150,
            FixRange = Duration.DurationInHours
        };

        protected DbSkill skill1_level1 = new DbSkill()
        {
            SkillId = 0,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            KeepTime = 3000 // 3 sec
        };

        protected DbSkill skill1_level2 = new DbSkill()
        {
            SkillId = 0,
            SkillLevel = 2,
            TypeDetail = TypeDetail.Buff,
            KeepTime = 5000 // 5 sec
        };

        protected DbSkill BlastAbsorbRedemySkill = new DbSkill()
        {
            SkillId = 418,
            SkillLevel = 11,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.AbsorptionAura,
            AbilityValue1 = 20,
            KeepTime = 8
        };

        protected DbSkill Untouchable = new DbSkill()
        {
            SkillId = 655,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Untouchable,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.Caster,
            KeepTime = 3
        };

        protected DbSkill BullsEye = new DbSkill()
        {
            SkillId = 724,
            SkillLevel = 1,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.SelectedEnemy
        };

        protected DbSkill Stealth = new DbSkill()
        {
            SkillId = 63,
            SkillLevel = 1,
            SuccessValue = 100,
            TypeDetail = TypeDetail.Stealth,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            TypeEffect = TypeEffect.Buff,
            KeepTime = 10
        };

        protected DbSkill SpeedRemedy_Lvl1 = new DbSkill()
        {
            SkillId = 249,
            SkillLevel = 1,
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.MoveSpeed,
            AbilityValue1 = 1,
            NeedWeapon1 = 1,
            NeedWeapon2 = 1,
            NeedShield = 1,
            KeepTime = 600
        };

        protected DbSkill MinSunStone_Lvl1 = new DbSkill()
        {
            SkillId = 250,
            SkillLevel = 1,
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.ExpGainRate,
            AbilityValue1 = 150,
            KeepTime = 3600
        };

        protected DbSkill BlueDragonCharm_Lvl1 = new DbSkill()
        {
            SkillId = 231,
            SkillLevel = 1,
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.BlueDragonCharm,
            AbilityValue1 = 1,
            KeepTime = 240
        };

        protected DbSkill DoubleWarehouseStone_Lvl1 = new DbSkill()
        {
            SkillId = 234,
            SkillLevel = 1,
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.WarehouseSize,
            AbilityValue1 = 1,
            KeepTime = 720
        };

        protected DbSkill MainWeaponPowerUp = new DbSkill()
        {
            SkillId = 613,
            SkillLevel = 1,
            TypeDetail = TypeDetail.WeaponPowerUp,
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 2,
            Weaponvalue = 35
        };

        protected DbSkill FleetFoot = new DbSkill()
        {
            SkillId = 711,
            SkillLevel = 10,
            TypeDetail = TypeDetail.BlockShootingAttack,
            DefenceValue = 100,
            KeepTime = 18
        };

        protected DbSkill Transformation = new DbSkill()
        {
            SkillId = 460,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Transformation,
            KeepTime = 20
        };

        protected DbSkill BerserkersRage = new DbSkill()
        {
            SkillId = 623,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            KeepTime = 10,
            AbilityType1 = AbilityType.SacrificeHPPercent,
            AbilityValue1 = 8,
            LimitHP = 10
        };

        protected DbSkill WildRage = new DbSkill()
        {
            SkillId = 630,
            SkillLevel = 1,
            TypeDetail = TypeDetail.UniqueHitAttack,
            TypeAttack = TypeAttack.PhysicalAttack,
            DamageType = DamageType.PlusExtraDamage,
            DamageHP = 235,
            DamageSP = 47
        };

        protected DbSkill DeadlyStrike = new DbSkill()
        {
            SkillId = 627,
            SkillLevel = 1,
            TypeDetail = TypeDetail.MultipleHitsAttack,
            TargetType = TargetType.SelectedEnemy,
            TypeAttack = TypeAttack.PhysicalAttack,
            DamageType = DamageType.PlusExtraDamage,
            MultiAttack = 2
        };

        protected DbSkill NettleSting = new DbSkill()
        {
            SkillId = 340,
            SkillLevel = 1,
            TypeDetail = TypeDetail.UniqueHitAttack,
            DamageType = DamageType.PlusExtraDamage,
            DamageHP = 285,
            TargetType = TargetType.EnemiesNearTarget,
            TypeAttack = TypeAttack.PhysicalAttack,
            NeedWeapon6 = 1,
            TypeEffect = TypeEffect.BasicDamage
        };

        protected DbSkill Eraser = new DbSkill()
        {
            SkillId = 631,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Eraser,
            DamageType = DamageType.Eraser,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100
        };

        protected DbSkill BloodyArc = new DbSkill()
        {
            SkillId = 636,
            SkillLevel = 1,
            TypeDetail = TypeDetail.SubtractingDebuff,
            DamageType = DamageType.PlusExtraDamage,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.EnemiesNearCaster,
            ApplyRange = 2,
            TypeAttack = TypeAttack.PhysicalAttack
        };

        protected DbSkill IntervalTraining = new DbSkill()
        {
            SkillId = 615,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PassiveDefence,
            AbilityType1 = AbilityType.SacrificeStr,
            AbilityValue1 = 3,
            AbilityType2 = AbilityType.IncreaseDexBySacrificing,
            AbilityValue2 = 25,
            TypeAttack = TypeAttack.Passive
        };

        protected DbSkill MagicVeil = new DbSkill()
        {
            SkillId = 772,
            SkillLevel = 1,
            TypeDetail = TypeDetail.BlockMagicAttack,
            DefenceValue = 3,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            KeepTime = 60
        };

        protected DbSkill EtainsEmbrace = new DbSkill()
        {
            SkillId = 775,
            SkillLevel = 1,
            TypeDetail = TypeDetail.EtainShield,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            MP = 850,
            TargetType = TargetType.Caster,
            TypeEffect = TypeEffect.Buff,
            TypeAttack = TypeAttack.MagicAttack,
            KeepTime = 2
        };

        protected DbSkill MagicMirror = new DbSkill()
        {
            SkillId = 776,
            SkillLevel = 1,
            TypeDetail = TypeDetail.DamageReflection,
            AbilityValue3 = 1,
            TargetType = TargetType.Caster,
            KeepTime = 2
        };

        protected DbSkill PersistBarrier = new DbSkill()
        {
            SkillId = 779,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PersistBarrier,
            TargetType = TargetType.Caster,
            TypeEffect = TypeEffect.Buff,
            AbilityType1 = AbilityType.SacrificeMPPercent,
            AbilityValue1 = 7,
            DamageType = DamageType.FixedDamage,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            ApplyRange = 4,
            KeepTime = 1
        };

        protected DbSkill Healing = new DbSkill()
        {
            SkillId = 785,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Healing,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            HealHP = 50
        };

        protected DbSkill FrostBarrier = new DbSkill()
        {
            SkillId = 364,
            SkillLevel = 1,
            TypeDetail = TypeDetail.FireThorn,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageHP = 85,
            ApplyRange = 3
        };

        protected DbSkill Resurrection = new DbSkill()
        {
            SkillId = 792,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Resurrection,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack
        };

        protected DbSkill Hypnosis = new DbSkill()
        {
            SkillId = 777,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Stun,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Debuff,
            StateType = StateType.Sleep,
            KeepTime = 8
        };

        protected DbSkill Evolution = new DbSkill()
        {
            SkillId = 793,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Evolution,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Buff,
            UsedByPriest = 1,
            KeepTime = 60
        };

        protected DbSkill Polymorph = new DbSkill()
        {
            SkillId = 794,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Evolution,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Debuff,
            UsedByPriest = 1,
            KeepTime = 5
        };

        protected DbSkill Detection = new DbSkill()
        {
            SkillId = 791,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Detection,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            ApplyRange = 10
        };

        protected DbSkill HealingPrayer = new DbSkill()
        {
            SkillId = 786,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Healing,
            TargetType = TargetType.AlliesButCaster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            ApplyRange = 10,
            HealHP = 80,
            TypeEffect = TypeEffect.HealingDispel
        };

        protected DbSkill TransformationAssassin = new DbSkill()
        {
            SkillId = 677,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Evolution,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Buff,
            UsedByRanger = 1,
            KeepTime = 60
        };

        protected DbSkill Disguise = new DbSkill()
        {
            SkillId = 680,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Evolution,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Buff,
            UsedByRanger = 1,
            KeepTime = 200
        };

        protected DbSkill Misfortune = new DbSkill()
        {
            SkillId = 702,
            SkillLevel = 1,
            TypeDetail = TypeDetail.SubtractingDebuff,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TypeAttack = TypeAttack.MagicAttack,
            TypeEffect = TypeEffect.Debuff,
            StateType = StateType.Misfortunate,
            AbilityType1 = AbilityType.Luc,
            AbilityValue1 = 166,
            AbilityType2 = AbilityType.Dex,
            AbilityValue2 = 166,
            KeepTime = 25
        };

        protected DbSkill StunSlam = new DbSkill()
        {
            SkillId = 693,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Stun,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TypeAttack = TypeAttack.PhysicalAttack,
            TypeEffect = TypeEffect.Debuff,
            StateType = StateType.Stun,
            KeepTime = 2
        };

        protected DbSkill DeathTouch = new DbSkill()
        {
            SkillId = 701,
            SkillLevel = 1,
            TypeDetail = TypeDetail.UniqueHitAttack,
            TargetType = TargetType.SelectedEnemy,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TypeAttack = TypeAttack.PhysicalAttack,
            DamageType = DamageType.HPPercentageDamage,
            DamageHP = 65
        };

        protected DbSkill PhantomAssault = new DbSkill()
        {
            SkillId = 692,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Sleep,
            TypeAttack = TypeAttack.PhysicalAttack,
            DamageType = DamageType.PlusExtraDamage,
            StateType = StateType.Sleep,
            TargetType = TargetType.EnemiesNearTarget,
            ApplyRange = 2,
            KeepTime = 2
        };

        protected DbSkill DisruptionStun = new DbSkill()
        {
            SkillId = 696,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Stun,
            TypeAttack = TypeAttack.PhysicalAttack,
            DamageType = DamageType.PlusExtraDamage,
            StateType = StateType.Stun,
            TargetType = TargetType.EnemiesNearCaster,
            ApplyRange = 2,
            KeepTime = 2
        };

        protected DbSkill PotentialForce = new DbSkill()
        {
            SkillId = 717,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PotentialDefence,
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.MagicResistance,
            AbilityValue1 = 20,
            LimitHP = 10
        };

        protected DbSkill Diversion = new DbSkill()
        {
            SkillId = 742,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Healing,
            TypeAttack = TypeAttack.MagicAttack,
            HealMP = 180,
            SP = 9,
            TargetType = TargetType.Caster
        };

        protected DbSkill UltimateBarrier = new DbSkill()
        {
            SkillId = 741,
            SkillLevel = 2,
            TypeDetail = TypeDetail.HealthAssistant,
            TypeEffect = TypeEffect.Buff,
            TargetType = TargetType.Caster,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.MagicAttack,
            KeepTime = 40
        };

        protected DbSkill LongRange = new DbSkill()
        {
            SkillId = 710,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Buff,
            TypeEffect = TypeEffect.Buff,
            TargetType = TargetType.Caster,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.AttackRange,
            AbilityValue1 = 6,
            KeepTime = 60
        };

        protected DbSkill HealthDrain = new DbSkill()
        {
            SkillId = 661,
            SkillLevel = 1,
            TypeDetail = TypeDetail.EnergyDrain,
            TypeEffect = TypeEffect.BasicDamage,
            DamageType = DamageType.FixedDamage,
            TypeAttack = TypeAttack.PhysicalAttack,
            TargetType = TargetType.SelectedEnemy,
            DamageHP = 68
        };

        protected DbSkill SilencingBlow = new DbSkill()
        {
            SkillId = 664,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PreventAttack,
            TypeEffect = TypeEffect.Debuff,
            DamageType = DamageType.PlusExtraDamage,
            TypeAttack = TypeAttack.PhysicalAttack,
            TargetType = TargetType.EnemiesNearTarget,
            StateType = StateType.Darkness,
            KeepTime = 4
        };

        protected DbSkill BlindingBlow = new DbSkill()
        {
            SkillId = 665,
            SkillLevel = 1,
            TypeDetail = TypeDetail.PreventAttack,
            TypeEffect = TypeEffect.Debuff,
            DamageType = DamageType.PlusExtraDamage,
            TypeAttack = TypeAttack.PhysicalAttack,
            TargetType = TargetType.EnemiesNearTarget,
            StateType = StateType.Silence,
            KeepTime = 3
        };

        protected DbSkill Provoke = new DbSkill()
        {
            SkillId = 305,
            SkillLevel = 1,
            TypeDetail = TypeDetail.Provoke,
            TypeEffect = TypeEffect.Debuff,
            TargetType = TargetType.SelectedEnemy,
            KeepTime = 1
        };

        #endregion

        #region Items

        protected DbItem WaterArmor = new DbItem()
        {
            Type = 17,
            TypeId = 2,
            ItemName = "Water armor",
            Element = Element.Water1,
            Count = 1,
            Quality = 1200
        };

        protected DbItem FireSword = new DbItem()
        {
            Type = 2,
            TypeId = 92,
            ItemName = "Thane Breaker of Fire",
            Element = Element.Fire1,
            Count = 1,
            Quality = 1200,
            Buy = 100
        };

        protected DbItem Spear = new DbItem()
        {
            Type = 6,
            TypeId = 1
        };

        protected DbItem PerfectLinkingHammer = new DbItem()
        {
            Type = 100,
            TypeId = 192,
            ItemName = "Perfect Linking Hammer",
            Special = SpecialEffect.PerfectLinkingHammer,
            Count = 255,
            Quality = 0,
            Country = ItemClassType.AllFactions
        };

        protected DbItem PerfectExtractingHammer = new DbItem()
        {
            Type = 44,
            TypeId = 237,
            ItemName = "GM Extraction Hammer",
            Special = SpecialEffect.PerfectExtractionHammer,
            Count = 10,
            Quality = 0,
            Country = ItemClassType.AllFactions
        };

        protected DbItem LuckyCharm = new DbItem()
        {
            Type = 100,
            TypeId = 139,
            ItemName = "Lucky Charm",
            Special = SpecialEffect.LuckyCharm,
            Count = 255,
            Quality = 0,
            Country = ItemClassType.AllFactions
        };

        protected DbItem JustiaArmor = new DbItem()
        {
            Type = 17,
            TypeId = 59,
            ItemName = "Justia Armor",
            ConstStr = 30,
            ConstDex = 30,
            ConstRec = 30,
            ConstHP = 1800,
            ConstMP = 50,
            ConstSP = 600,
            Slot = 6,
            Quality = 1200,
            Attackfighter = 1,
            Defensefighter = 1,
            ReqWis = 20,
            Count = 1
        };

        protected DbItem Gem_Str_Level_1 = new DbItem()
        {
            Type = 30,
            TypeId = 1,
            ConstStr = 3,
            ReqIg = 0, // always fail linking or extracting, unless hammer is used
            Count = 255,
            Quality = 0
        };

        protected DbItem Gem_Str_Level_2 = new DbItem()
        {
            Type = 30,
            TypeId = 2,
            ConstStr = 5,
            ReqIg = 255, // always success linking or extracting.
            Count = 255,
            Quality = 0
        };

        protected DbItem Gem_Str_Level_3 = new DbItem()
        {
            Type = 30,
            TypeId = 3,
            ConstStr = 7,
            ReqIg = 255, // always success linking or extracting.
            Count = 255,
            Quality = 0
        };

        protected DbItem Gem_Str_Level_7 = new DbItem()
        {
            Type = 30,
            TypeId = 7,
            ConstStr = 50,
            ReqVg = 1, // Will break item if linking/extracting fails
            ReqIg = 0, // always fail linking or extracting, unless hammer is used
            Count = 255,
            Quality = 0
        };

        protected DbItem Gem_Absorption_Level_4 = new DbItem()
        {
            Type = 30,
            TypeId = 240,
            Exp = 20
        };

        protected DbItem Gem_Absorption_Level_5 = new DbItem()
        {
            Type = 30,
            TypeId = 241,
            Exp = 50
        };

        protected DbItem EtainPotion = new DbItem()
        {
            Type = 100,
            TypeId = 1,
            ConstHP = 75,
            ConstMP = 75,
            ConstSP = 75,
            Special = SpecialEffect.PercentHealingPotion,
            Country = ItemClassType.AllFactions
        };

        protected DbItem RedApple = new DbItem()
        {
            Type = 25,
            TypeId = 1,
            Special = SpecialEffect.None,
            ConstHP = 50,
            ReqIg = 1,
            Country = ItemClassType.AllFactions,
            Count = 255,
            Grade = 1
        };

        protected DbItem GreenApple = new DbItem()
        {
            Type = 25,
            TypeId = 13,
            Special = SpecialEffect.None,
            ConstMP = 50,
            ReqIg = 1,
            Country = ItemClassType.AllFactions,
            Grade = 1
        };

        protected DbItem HorseSummonStone = new DbItem()
        {
            Type = 42,
            TypeId = 1
        };

        protected DbItem Nimbus1d = new DbItem()
        {
            Type = 42,
            TypeId = 136,
            Duration = 86400
        };

        protected DbItem Item_HealthRemedy_Level_1 = new DbItem()
        {
            Type = 100,
            TypeId = 95,
            Range = 256,
            AttackTime = 1,
            Country = ItemClassType.AllFactions
        };

        protected DbItem Item_AbsorbRemedy = new DbItem()
        {
            Type = 101,
            TypeId = 71,
            Range = 418,
            AttackTime = 11,
            Country = ItemClassType.AllFactions
        };

        protected DbItem SpeedyRemedy = new DbItem()
        {
            Type = 100,
            TypeId = 107,
            Special = SpecialEffect.None,
            Range = 249,
            AttackTime = 1,
            Country = ItemClassType.AllFactions
        };

        protected DbItem Etin_100 = new DbItem()
        {
            Type = 43,
            TypeId = 3,
            Special = SpecialEffect.Etin_100
        };

        protected DbItem PartySummonRune = new DbItem()
        {
            Type = 100,
            TypeId = 45,
            Special = SpecialEffect.PartySummon
        };

        protected DbItem MinSunExpStone = new DbItem()
        {
            Type = 100,
            TypeId = 108,
            Country = ItemClassType.AllFactions,
            Range = 250,
            AttackTime = 1
        };

        protected DbItem AssaultLapisia = new DbItem()
        {
            Type = 95,
            TypeId = 1,
            Special = SpecialEffect.Lapisia,
            Reqlevel = 1

        };

        protected DbItem ProtectorsLapisia = new DbItem()
        {
            Type = 95,
            TypeId = 6,
            Special = SpecialEffect.Lapisia,
            Country = ItemClassType.AllFactions
        };

        protected DbItem PerfectWeaponLapisia_Lvl1 = new DbItem()
        {
            Type = 95,
            TypeId = 22,
            Special = SpecialEffect.Lapisia,
            ReqRec = 10000,
            Range = 0,
            AttackTime = 1,
            Reqlevel = 1
        };

        protected DbItem PerfectWeaponLapisia_Lvl2 = new DbItem()
        {
            Type = 95,
            TypeId = 23,
            Special = SpecialEffect.Lapisia,
            ReqRec = 10000,
            Range = 1,
            AttackTime = 2,
            Reqlevel = 1
        };

        protected DbItem PerfectArmorLapisia_Lvl1 = new DbItem()
        {
            Type = 95,
            TypeId = 42,
            Special = SpecialEffect.Lapisia,
            ReqRec = 10000,
            Range = 0,
            AttackTime = 1,
            Country = ItemClassType.AllFactions
        };

        protected DbItem LapisiaBreakItem = new DbItem()
        {
            Type = 95,
            TypeId = 8,
            Special = SpecialEffect.Lapisia,
            ReqVg = 1,
            Country = ItemClassType.AllFactions
        };

        protected DbItem TeleportationStone = new DbItem()
        {
            Type = 100,
            TypeId = 65,
            Special = SpecialEffect.TeleportationStone,
            Country = ItemClassType.AllFactions
        };

        protected DbItem BlueDragonCharm = new DbItem()
        {
            Type = 100,
            TypeId = 72,
            Range = 231,
            AttackTime = 1,
            Special = SpecialEffect.None,
            Country = ItemClassType.AllFactions
        };

        protected DbItem BoxWithApples = new DbItem()
        {
            Type = 100,
            TypeId = 78,
            Special = SpecialEffect.AnotherItemGenerator,
            ReqVg = 1,
            Country = ItemClassType.AllFactions
        };

        protected DbItem DoubleWarehouse = new DbItem()
        {
            Type = 100,
            TypeId = 75,
            Country = ItemClassType.AllFactions,
            Range = 234,
            AttackTime = 1
        };

        #endregion

        #region Levels

        protected DbLevel Level1_Mode1 = new DbLevel()
        {
            Level = 1,
            Mode = Mode.Beginner,
            Exp = 70
        };

        protected DbLevel Level1_Mode2 = new DbLevel()
        {
            Level = 1,
            Mode = Mode.Normal,
            Exp = 200
        };

        protected DbLevel Level1_Mode3 = new DbLevel()
        {
            Level = 1,
            Mode = Mode.Hard,
            Exp = 200
        };

        protected DbLevel Level1_Mode4 = new DbLevel()
        {
            Level = 1,
            Mode = Mode.Ultimate,
            Exp = 200
        };

        protected DbLevel Level2_Mode1 = new DbLevel()
        {
            Level = 2,
            Mode = Mode.Beginner,
            Exp = 130
        };

        protected DbLevel Level2_Mode2 = new DbLevel()
        {
            Level = 2,
            Mode = Mode.Normal,
            Exp = 400
        };

        protected DbLevel Level2_Mode3 = new DbLevel()
        {
            Level = 2,
            Mode = Mode.Hard,
            Exp = 400
        };

        protected DbLevel Level2_Mode4 = new DbLevel()
        {
            Level = 2,
            Mode = Mode.Ultimate,
            Exp = 400
        };

        protected DbLevel Level37_Mode1 = new DbLevel()
        {
            Level = 37,
            Mode = Mode.Beginner,
            Exp = 171200
        };

        protected DbLevel Level37_Mode2 = new DbLevel()
        {
            Level = 37,
            Mode = Mode.Normal,
            Exp = 2418240
        };

        protected DbLevel Level37_Mode3 = new DbLevel()
        {
            Level = 37,
            Mode = Mode.Hard,
            Exp = 2418240
        };

        protected DbLevel Level37_Mode4 = new DbLevel()
        {
            Level = 37,
            Mode = Mode.Ultimate,
            Exp = 3022800
        };

        protected DbLevel Level38_Mode1 = new DbLevel()
        {
            Level = 38,
            Mode = Mode.Beginner,
            Exp = 171200
        };

        protected DbLevel Level38_Mode2 = new DbLevel()
        {
            Level = 38,
            Mode = Mode.Normal,
            Exp = 2714880
        };

        protected DbLevel Level38_Mode3 = new DbLevel()
        {
            Level = 38,
            Mode = Mode.Hard,
            Exp = 2714880
        };

        protected DbLevel Level38_Mode4 = new DbLevel()
        {
            Level = 38,
            Mode = Mode.Ultimate,
            Exp = 3396800
        };

        protected DbLevel Level79_Mode1 = new DbLevel()
        {
            Level = 79,
            Mode = Mode.Beginner,
            Exp = 171200
        };

        protected DbLevel Level79_Mode2 = new DbLevel()
        {
            Level = 79,
            Mode = Mode.Normal,
            Exp = 214847083
        };

        protected DbLevel Level79_Mode3 = new DbLevel()
        {
            Level = 69,
            Mode = Mode.Hard,
            Exp = 214847083
        };

        protected DbLevel Level79_Mode4 = new DbLevel()
        {
            Level = 79,
            Mode = Mode.Ultimate,
            Exp = 330854048
        };

        protected DbLevel Level80_Mode1 = new DbLevel()
        {
            Level = 50,
            Mode = Mode.Beginner,
            Exp = 171200
        };

        protected DbLevel Level80_Mode2 = new DbLevel()
        {
            Level = 60,
            Mode = Mode.Normal,
            Exp = 214847083
        };

        protected DbLevel Level80_Mode3 = new DbLevel()
        {
            Level = 70,
            Mode = Mode.Hard,
            Exp = 214847083
        };

        protected DbLevel Level80_Mode4 = new DbLevel()
        {
            Level = 80,
            Mode = Mode.Ultimate,
            Exp = 330854048
        };

        #endregion

        #region NPC

        protected TestNpc WeaponMerchant = new TestNpc()
        {
            Type = NpcType.Merchant,
            TypeId = 1,
            Name = "Erina Probicio",
            MerchantType = MerchantType.WeaponMerchant
        };

        #endregion

        #region Quest

        protected SQuest NewBeginnings = new SQuest()
        {
            Id = 3400,
            Name = "New Beginnings",
            RequiredMobId1 = 2011,
            RequiredMobCount1 = 5
        };

        protected SQuest Bartering = new SQuest()
        {
            Id = 3401,
            Name = "Bartering"
        };

        protected SQuest SkillsAndStats = new SQuest()
        {
            Id = 3782,
            Name = "Skills and Stats"
        };

        #endregion
    }
}
