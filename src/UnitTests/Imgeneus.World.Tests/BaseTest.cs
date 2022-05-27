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

        protected Map testMap => new Map(
                    Map.TEST_MAP_ID,
                    new MapDefinition(),
                    new Svmap() { MapSize = 100, CellSize = 100 },
                    new List<ObeliskConfiguration>(),
                    mapLoggerMock.Object,
                    packetFactoryMock.Object,
                    databasePreloader.Object,
                    mobFactoryMock.Object,
                    npcFactoryMock.Object,
                    obeliskFactoryMock.Object,
                    timeMock.Object);

        private int _characterId;
        protected Character CreateCharacter(Map map = null, Fraction country = Fraction.Light, GuildConfiguration guildConfiguration = null, GuildHouseConfiguration guildHouseConfiguration = null)
        {
            _characterId++;

            var countryProvider = new CountryProvider(new Mock<ILogger<CountryProvider>>().Object);
            countryProvider.Init(_characterId, country);

            var levelProvider = new LevelProvider(new Mock<ILogger<LevelProvider>>().Object);
            levelProvider.Init(_characterId, 1);

            var mapProvider = new MapProvider(new Mock<ILogger<MapProvider>>().Object);
            var speedManager = new SpeedManager(new Mock<ILogger<SpeedManager>>().Object);
            var additionalInfoManager = new AdditionalInfoManager(new Mock<ILogger<AdditionalInfoManager>>().Object, config.Object, databaseMock.Object);
            var statsManager = new StatsManager(new Mock<ILogger<StatsManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object);

            var healthManager = new HealthManager(new Mock<ILogger<HealthManager>>().Object, statsManager, levelProvider, config.Object, databaseMock.Object);
            healthManager.Init(_characterId, 0, 0, 0, null, null, null, CharacterProfession.Fighter);

            var elementProvider = new ElementProvider(new Mock<ILogger<ElementProvider>>().Object);
            var untouchableManager = new UntouchableManager(new Mock<ILogger<UntouchableManager>>().Object);
            var stealthManager = new StealthManager(new Mock<ILogger<StealthManager>>().Object);
            var movementManager = new MovementManager(new Mock<ILogger<MovementManager>>().Object);

            var partyManager = new PartyManager(new Mock<ILogger<PartyManager>>().Object, packetFactoryMock.Object, gameWorldMock.Object, mapProvider, healthManager);
            partyManager.Init(_characterId);

            var levelingManager = new LevelingManager(new Mock<ILogger<LevelingManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object, databasePreloader.Object, partyManager, mapProvider, movementManager);
            levelingManager.Init(_characterId, 0);

            var teleportManager = new TeleportationManager(new Mock<ILogger<TeleportationManager>>().Object, movementManager, mapProvider, databaseMock.Object, countryProvider, levelProvider, gameWorldMock.Object, healthManager);
            teleportManager.Init(_characterId, new List<DbCharacterSavePositions>());

            var warehouseManager = new WarehouseManager(new Mock<ILogger<WarehouseManager>>().Object, databaseMock.Object, databasePreloader.Object, enchantConfig.Object, itemCreateConfig.Object, gameWorldMock.Object, packetFactoryMock.Object);

            var attackManager = new AttackManager(new Mock<ILogger<AttackManager>>().Object, statsManager, levelProvider, elementProvider, countryProvider, speedManager, stealthManager);
            var buffsManager = new BuffsManager(new Mock<ILogger<BuffsManager>>().Object, databaseMock.Object, databasePreloader.Object, statsManager, healthManager, speedManager, elementProvider, untouchableManager, stealthManager, levelingManager, attackManager, teleportManager, warehouseManager);

            var skillsManager = new SkillsManager(new Mock<ILogger<SkillsManager>>().Object, databasePreloader.Object, databaseMock.Object, healthManager, attackManager, buffsManager, statsManager, elementProvider, countryProvider, config.Object, levelProvider, additionalInfoManager, gameWorldMock.Object, mapProvider);
            var vehicleManager = new VehicleManager(new Mock<ILogger<VehicleManager>>().Object, stealthManager, speedManager, healthManager, gameWorldMock.Object);

            var inventoryManager = new InventoryManager(new Mock<ILogger<InventoryManager>>().Object, databasePreloader.Object, definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, databaseMock.Object, statsManager, healthManager, speedManager, elementProvider, vehicleManager, levelProvider, levelingManager, countryProvider, gameWorldMock.Object, additionalInfoManager, skillsManager, buffsManager, config.Object, attackManager, partyManager, teleportManager, new Mock<IChatManager>().Object, warehouseManager);
            inventoryManager.Init(_characterId, new List<DbCharacterItems>(), 0);

            var killsManager = new KillsManager(new Mock<ILogger<KillsManager>>().Object, databaseMock.Object);
            var shapeManager = new ShapeManager(new Mock<ILogger<ShapeManager>>().Object, stealthManager, vehicleManager, inventoryManager);
            var guildManager = new GuildManager(new Mock<ILogger<GuildManager>>().Object, guildConfiguration, guildHouseConfiguration, databaseMock.Object, gameWorldMock.Object, timeMock.Object, inventoryManager, partyManager, countryProvider, etinMock.Object);
            guildManager.Init(_characterId);

            var linkingManager = new LinkingManager(new Mock<ILogger<LinkingManager>>().Object, databasePreloader.Object, inventoryManager, statsManager, healthManager, speedManager, guildManager, mapProvider, enchantConfig.Object, itemCreateConfig.Object);
            var tradeManager = new TradeManager(new Mock<ILogger<TradeManager>>().Object, gameWorldMock.Object, inventoryManager);
            var friendsManager = new FriendsManager(new Mock<ILogger<FriendsManager>>().Object, databaseMock.Object, gameWorldMock.Object);
            var duelManager = new DuelManager(new Mock<ILogger<DuelManager>>().Object, gameWorldMock.Object, tradeManager, movementManager, healthManager, killsManager, mapProvider, inventoryManager, teleportManager);
            var bankManager = new BankManager(new Mock<ILogger<BankManager>>().Object, databaseMock.Object, databasePreloader.Object, enchantConfig.Object, itemCreateConfig.Object, inventoryManager);
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, mapProvider, gameWorldMock.Object, databaseMock.Object, partyManager, inventoryManager, databasePreloader.Object, enchantConfig.Object, itemCreateConfig.Object, levelingManager);
            var shopManager = new ShopManager(new Mock<ILogger<ShopManager>>().Object, inventoryManager, mapProvider);

            var character = new Character(
                new Mock<ILogger<Character>>().Object,
                databasePreloader.Object,
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
                gameSessionMock.Object,
                packetFactoryMock.Object);


            character.Id = _characterId;

            if (map != null)
                map.LoadPlayer(character);

            gameWorldMock.Object.Players.TryAdd(character.Id, character);

            return character;
        }

        private int _mobId;

        protected Mob CreateMob(ushort mobId, Map map, Fraction country = Fraction.NotSelected)
        {
            _mobId++;

            var countryProvider = new CountryProvider(new Mock<ILogger<CountryProvider>>().Object);
            countryProvider.Init(0, country);

            var mapProvider = new MapProvider(new Mock<ILogger<MapProvider>>().Object);
            var levelProvider = new LevelProvider(new Mock<ILogger<LevelProvider>>().Object);
            levelProvider.Init(_mobId, databasePreloader.Object.Mobs[mobId].Level);

            var levelingManager = new Mock<ILevelingManager>();
            var additionalInfoManager = new AdditionalInfoManager(new Mock<ILogger<AdditionalInfoManager>>().Object, config.Object, databaseMock.Object);
            var statsManager = new StatsManager(new Mock<ILogger<StatsManager>>().Object, databaseMock.Object, levelProvider, additionalInfoManager, config.Object);
            var healthManager = new HealthManager(new Mock<ILogger<HealthManager>>().Object, statsManager, levelProvider, config.Object, databaseMock.Object);
            var speedManager = new SpeedManager(new Mock<ILogger<SpeedManager>>().Object);
            var elementProvider = new ElementProvider(new Mock<ILogger<ElementProvider>>().Object);
            var untouchableManager = new UntouchableManager(new Mock<ILogger<UntouchableManager>>().Object);
            var stealthManager = new StealthManager(new Mock<ILogger<StealthManager>>().Object);
            var attackManager = new AttackManager(new Mock<ILogger<AttackManager>>().Object, statsManager, levelProvider, elementProvider, countryProvider, speedManager, stealthManager);
            var buffsManager = new BuffsManager(new Mock<ILogger<BuffsManager>>().Object, databaseMock.Object, databasePreloader.Object, statsManager, healthManager, speedManager, elementProvider, untouchableManager, stealthManager, levelingManager.Object, attackManager, null, null);
            var skillsManager = new SkillsManager(new Mock<ILogger<SkillsManager>>().Object, databasePreloader.Object, databaseMock.Object, healthManager, attackManager, buffsManager, statsManager, elementProvider, countryProvider, config.Object, levelProvider, additionalInfoManager, gameWorldMock.Object, mapProvider);
            var movementManager = new MovementManager(new Mock<ILogger<MovementManager>>().Object);
            var aiManager = new AIManager(new Mock<ILogger<AIManager>>().Object, movementManager, countryProvider, attackManager, untouchableManager, mapProvider, skillsManager, statsManager, elementProvider, databasePreloader.Object);

            var mob = new Mob(
                mobId,
                true,
                new MoveArea(0, 0, 0, 0, 0, 0),
                mobLoggerMock.Object,
                databasePreloader.Object,
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

            return mob;
        }

        public BaseTest()
        {
            gameWorldMock.Setup(x => x.Players)
                .Returns(new ConcurrentDictionary<int, Character>());

            PortalTeleportNotAllowedReason reason;
            gameWorldMock.Setup(x => x.CanTeleport(It.IsAny<Character>(), It.IsAny<ushort>(), out reason))
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
                .SetupGet((preloader) => preloader.Mobs)
                .Returns(new Dictionary<ushort, DbMob>()
                {
                    { 1, Wolf },
                    { 3041, CrypticImmortal }
                });

            databasePreloader
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
                    { (613, 1), MainWeaponPowerUp }
                });
            databasePreloader
                .SetupGet((preloader) => preloader.Items)
                .Returns(new Dictionary<(byte Type, byte TypeId), DbItem>()
                {
                    { (17, 2), WaterArmor },
                    { (2, 92), FireSword },
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
                    { (100, 75), DoubleWarehouse }
                });

            databasePreloader
                .SetupGet((preloader) => preloader.ItemsByGrade)
                .Returns(new Dictionary<ushort, List<DbItem>>() {
                    { 1, new List<DbItem>() { RedApple, GreenApple } }
                });

            databasePreloader
                .SetupGet((preloader) => preloader.MobItems)
                .Returns(new Dictionary<(ushort MobId, byte ItemOrder), DbMobItems>());

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
                .SetupGet((preloader) => preloader.NPCs)
                .Returns(new Dictionary<(NpcType Type, short TypeId), BaseNpc>()
                {
                    { (NpcType.Merchant, 1), WeaponMerchant }
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
            MobName = "Small Ruined Wolf",
            AI = MobAI.Combative,
            Level = 38,
            HP = 2765,
            Element = Element.Wind1,
            AttackSpecial3 = MobRespawnTime.TestEnv,
            NormalTime = 1,
            Exp = 70
        };

        protected DbMob CrypticImmortal = new DbMob()
        {
            Id = 3041,
            MobName = "Cryptic the Immortal",
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
            SkillName = "Strength Training Lv1",
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
            SkillName = "Mana Training",
            TypeAttack = TypeAttack.Passive,
            AbilityType1 = AbilityType.MP,
            AbilityValue1 = 110
        };

        protected DbSkill SharpenWeaponMastery_Lvl1 = new DbSkill()
        {
            SkillId = 15,
            SkillLevel = 1,
            TypeDetail = TypeDetail.WeaponMastery,
            SkillName = "Sharpen Weapon Mastery Lvl 1",
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
            SkillName = "Sharpen Weapon Mastery Lvl 2",
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
            SkillName = "Magic Roots",
            DamageHP = 42,
            TypeAttack = TypeAttack.MagicAttack,
            ResetTime = 10,
            KeepTime = 5,
            DamageType = DamageType.PlusExtraDamage,
        };

        protected DbSkill AttributeRemove = new DbSkill()
        {
            SkillId = 273,
            SkillLevel = 100,
            TypeDetail = TypeDetail.RemoveAttribute,
            SkillName = "Attribute Remove",
            TypeAttack = TypeAttack.MagicAttack,
            DamageType = DamageType.FixedDamage
        };

        protected DbSkill FireWeapon = new DbSkill()
        {
            SkillId = 732,
            SkillLevel = 1,
            SkillName = "Flame Weapon",
            TypeDetail = TypeDetail.ElementalAttack,
            Element = Element.Fire1,
            TypeAttack = TypeAttack.ShootingAttack
        };

        protected DbSkill EarthWeapon = new DbSkill()
        {
            SkillId = 735,
            SkillLevel = 1,
            SkillName = "Earth Weapon",
            TypeDetail = TypeDetail.ElementalAttack,
            Element = Element.Earth1,
            TypeAttack = TypeAttack.ShootingAttack
        };

        protected DbSkill FireSkin = new DbSkill()
        {
            SkillId = 762,
            SkillLevel = 1,
            SkillName = "Flame Skin",
            TypeDetail = TypeDetail.ElementalProtection,
            Element = Element.Fire1,
            TypeAttack = TypeAttack.MagicAttack
        };

        protected DbSkill EarthSkin = new DbSkill()
        {
            SkillId = 765,
            SkillLevel = 1,
            SkillName = "Earth Skin",
            TypeDetail = TypeDetail.ElementalProtection,
            Element = Element.Earth1,
            TypeAttack = TypeAttack.MagicAttack
        };

        protected DbSkill Panic_Lvl1 = new DbSkill()
        {
            SkillId = 672,
            SkillLevel = 1,
            SkillName = "Panic",
            TypeDetail = TypeDetail.SubtractingDebuff,
            AbilityType1 = AbilityType.PhysicalDefense,
            AbilityValue1 = 119,
            TypeAttack = TypeAttack.MagicAttack,
        };

        protected DbSkill Dispel = new DbSkill()
        {
            SkillId = 787,
            SkillLevel = 1,
            SkillName = "Dispel",
            TypeDetail = TypeDetail.Dispel,
            TypeAttack = TypeAttack.MagicAttack,
        };

        protected DbSkill Skill_HealthRemedy_Level1 = new DbSkill()
        {
            SkillId = 256,
            SkillLevel = 1,
            SkillName = "Health Remedy Lv1",
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.HP,
            AbilityValue1 = 500,
            FixRange = ClearAfterDeath.KeepInMins
        };

        protected DbSkill Leadership = new DbSkill()
        {
            SkillId = 112,
            SkillLevel = 1,
            SkillName = "Leadership Lv1",
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.AlliesNearCaster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            ApplyRange = 50,
            AbilityType1 = AbilityType.PhysicalAttackPower,
            AbilityValue1 = 13,
            FixRange = ClearAfterDeath.Clear
        };

        protected DbSkill EXP = new DbSkill()
        {
            SkillId = 222,
            SkillLevel = 1,
            SkillName = "Increase EXP",
            TypeDetail = TypeDetail.Buff,
            TargetType = TargetType.Caster,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            ApplyRange = 50,
            AbilityType1 = AbilityType.ExpGainRate,
            AbilityValue1 = 150,
            FixRange = ClearAfterDeath.KeepInHours
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
            AbilityValue1 = 20
        };

        protected DbSkill Untouchable = new DbSkill()
        {
            SkillId = 655,
            SkillLevel = 1,
            SkillName = "Untouchable Lv1",
            TypeDetail = TypeDetail.Untouchable,
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.Caster
        };

        protected DbSkill BullsEye = new DbSkill()
        {
            SkillId = 724,
            SkillLevel = 1,
            SkillName = "Bull's Eye",
            SuccessType = SuccessType.SuccessBasedOnValue,
            SuccessValue = 100,
            TargetType = TargetType.SelectedEnemy
        };

        protected DbSkill Stealth = new DbSkill()
        {
            SkillId = 63,
            SkillLevel = 1,
            SkillName = "Stealth Lvl1",
            SuccessValue = 100,
            TypeDetail = TypeDetail.Stealth,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster
        };

        protected DbSkill SpeedRemedy_Lvl1 = new DbSkill()
        {
            SkillId = 249,
            SkillLevel = 1,
            SkillName = "Speedy Remedy",
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.MoveSpeed,
            AbilityValue1 = 1,
            NeedWeapon1 = 1,
            NeedWeapon2 = 1,
            NeedShield = 1
        };

        protected DbSkill MinSunStone_Lvl1 = new DbSkill()
        {
            SkillId = 250,
            SkillLevel = 1,
            SkillName = "Mini Sun EXP Stone",
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.ExpGainRate,
            AbilityValue1 = 150
        };

        protected DbSkill BlueDragonCharm_Lvl1 = new DbSkill()
        {
            SkillId = 231,
            SkillLevel = 1,
            SkillName = "Blue Dragon Charm",
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.BlueDragonCharm,
            AbilityValue1 = 1
        };

        protected DbSkill DoubleWarehouseStone_Lvl1 = new DbSkill()
        {
            SkillId = 234,
            SkillLevel = 1,
            SkillName = "Double Warehouse Stone",
            SuccessValue = 100,
            TypeDetail = TypeDetail.Buff,
            SuccessType = SuccessType.SuccessBasedOnValue,
            TargetType = TargetType.Caster,
            AbilityType1 = AbilityType.WarehouseSize,
            AbilityValue1 = 1
        };

        protected DbSkill MainWeaponPowerUp = new DbSkill()
        {
            SkillId = 613,
            SkillLevel = 1,
            SkillName = "Main Weapon Power Up",
            TypeDetail = TypeDetail.WeaponPowerUp,
            TypeAttack = TypeAttack.Passive,
            Weapon1 = 1,
            Weapon2 = 2,
            Weaponvalue = 35
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
