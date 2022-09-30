using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Etin;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Imgeneus.World.Tests.GuildTests
{
    public class GuildTest : BaseTest
    {
        private Mock<IInventoryManager> inventoryMock => new Mock<IInventoryManager>();
        private Mock<IPartyManager> partyMock => new Mock<IPartyManager>();
        private Mock<ICountryProvider> countryMock => new Mock<ICountryProvider>();
        private Mock<IEtinManager> etinMock => new Mock<IEtinManager>();
        private Mock<IServiceProvider> serviceMock => new Mock<IServiceProvider>();

        [Fact]
        [Description("It should not be possible to create a guild, if not enough money.")]
        public async Task CanCreateGuild_MinGoldTest()
        {
            var config = new GuildConfiguration()
            {
                MinGold = 10
            };

            var inventory = inventoryMock;
            inventory.Setup(x => x.Gold).Returns(0);

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, inventory.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceMock.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal((uint)0, inventory.Object.Gold);
            Assert.Equal(GuildCreateFailedReason.NotEnoughGold, result);
        }

        [Fact]
        [Description("It should not be possible to create a guild, if not enough party members.")]
        public async Task CanCreateGuild_MinMembersTest()
        {
            var config = new GuildConfiguration()
            {
                MinMembers = 2
            };

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, inventoryMock.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceMock.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal(GuildCreateFailedReason.NotEnoughMembers, result);
        }

        [Fact]
        [Description("It should not be possible to create a guild, if not enough party members level.")]
        public async Task CanCreateGuild_MinLevelTest()
        {
            var config = new GuildConfiguration()
            {
                MinMembers = 2,
                MinLevel = 2
            };

            var character1 = CreateCharacter(testMap);
            var character2 = CreateCharacter(testMap);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, inventoryMock.Object, character1.PartyManager, countryMock.Object, etinMock.Object, serviceMock.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal(1, character1.LevelProvider.Level);
            Assert.Equal(1, character2.LevelProvider.Level);

            Assert.Equal(GuildCreateFailedReason.LevelLimit, result);
        }

        [Fact]
        [Description("It should not be possible to create a guild, if one party meber belongs to another guild.")]
        public async Task CanCreateGuild_AnotherGuildTest()
        {
            var config = new GuildConfiguration()
            {
                MinMembers = 2
            };

            var character1 = CreateCharacter(testMap);
            var character2 = CreateCharacter(testMap);
            character2.GuildManager.SetGuildInfo(1, "", 9);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, inventoryMock.Object, character1.PartyManager, countryMock.Object, etinMock.Object, serviceMock.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal(GuildCreateFailedReason.PartyMemberInAnotherGuild, result);
        }

        [Fact]
        [Description("It should not be possible to create a guild with wrong name.")]
        public async Task CanCreateGuild_GuildNameTest()
        {
            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, inventoryMock.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceMock.Object);
            var result = await guildManager.CanCreateGuild("");

            Assert.Equal(GuildCreateFailedReason.WrongName, result);
        }

        [Fact]
        [Description("It should not be possible to create a guild if at least one party member has penalty.")]
        public async Task CanCreateGuild_PenaltyTest()
        {
            var config = new GuildConfiguration()
            {
                MinMembers = 2,
                MinPenalty = 2 // 2 hours
            };

            var character1 = CreateCharacter(testMap);
            var character2 = CreateCharacter(testMap);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var time = new Mock<ITimeService>();
            time.Setup(x => x.UtcNow)
                .Returns(new DateTime(2021, 1, 1, 2, 0, 0));  // 1 Jan 2021 02:00

            var database = new Mock<IDatabase>();
            database.Setup(x => x.Characters.FindAsync(character1.Id))
                .ReturnsAsync(new DbCharacter()
                {
                    Id = character1.Id,
                    GuildLeaveTime = new DateTime(2021, 1, 1, 1, 0, 0) // 1 Jan 2021 01:00
                });
            database.Setup(x => x.Characters.FindAsync(character2.Id))
                .ReturnsAsync(new DbCharacter());

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, time.Object, character1.InventoryManager, character1.PartyManager, character1.CountryProvider, etinMock.Object, serviceProvider.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal(GuildCreateFailedReason.PartyMemberGuildPenalty, result);
        }

        [Fact]
        [Description("It should be possible to create a guild.")]
        public async Task CanCreateGuild_SuccessTest()
        {
            var config = new GuildConfiguration()
            {
                MinMembers = 2
            };

            var character1 = CreateCharacter(testMap);
            var character2 = CreateCharacter(testMap);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var database = new Mock<IDatabase>();
            database.Setup(x => x.Characters.FindAsync(character1.Id)).ReturnsAsync(new DbCharacter());
            database.Setup(x => x.Characters.FindAsync(character2.Id)).ReturnsAsync(new DbCharacter());

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var guildManager = new GuildManager(guildLoggerMock.Object, config, new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character1.InventoryManager, character1.PartyManager, character1.CountryProvider, etinMock.Object, serviceProvider.Object);
            var result = await guildManager.CanCreateGuild("guild_name");

            Assert.Equal(GuildCreateFailedReason.Success, result);
        }

        [Fact]
        [Description("It should send guild create request to all party members. As soon as party changes, request is not valid.")]
        public void SendGuildRequest_PartyChangeTest()
        {
            var character1 = CreateCharacter(testMap, guildConfiguration: new GuildConfiguration() { MinMembers = 2 });
            var character2 = CreateCharacter(testMap, guildConfiguration: new GuildConfiguration() { MinMembers = 2 });

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            character1.GuildManager.InitCreateRequest("guild_name", "guild_message");

            Assert.NotNull(character1.GuildManager.CreationRequest);
            Assert.True(character1.GuildManager.CreationRequest.GuildCreatorId == character1.Id);

            character2.PartyManager.Party = null;

            Assert.Null(character1.GuildManager.CreationRequest);
        }

        [Fact]
        [Description("Player can buy guild house only if he is guild owner (i.e. rank is 1).")]
        public async void TryBuyHouse_OnlyRank1Test()
        {
            var character = CreateCharacter(testMap);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character.InventoryManager, character.PartyManager, character.CountryProvider, etinMock.Object, serviceMock.Object);

            var result = await guildManager.TryBuyHouse();

            Assert.Equal(GuildHouseBuyReason.NotAuthorized, result);
        }

        [Fact]
        [Description("Player can buy guild house if he has enough gold.")]
        public async void TryBuyHouse_NotEnoughtGoldTest()
        {
            var character = CreateCharacter(testMap);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration() { HouseBuyMoney = 100 }, gameWorldMock.Object, timeMock.Object, character.InventoryManager, character.PartyManager, character.CountryProvider, etinMock.Object, serviceMock.Object);
            guildManager.GuildMemberRank = 1;

            var result = await guildManager.TryBuyHouse();

            Assert.Equal(GuildHouseBuyReason.NoGold, result);
        }

        [Fact]
        [Description("Player can buy guild house if his guild is in top 30 rank.")]
        public async void TryBuyHouse_Top30Test()
        {
            var character = CreateCharacter(testMap);

            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.FindAsync(It.IsAny<uint>())).ReturnsAsync(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 31 });

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character.InventoryManager, character.PartyManager, character.CountryProvider, etinMock.Object, serviceProvider.Object);
            guildManager.SetGuildInfo(1, "", 1);

            var result = await guildManager.TryBuyHouse();

            Assert.Equal(GuildHouseBuyReason.LowRank, result);
        }

        [Fact]
        [Description("Player can buy guild house only once.")]
        public async void TryBuyHouse_HasHouseTest()
        {
            var character = CreateCharacter(testMap);

            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.FindAsync(It.IsAny<uint>())).ReturnsAsync(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 1, HasHouse = true });

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character.InventoryManager, character.PartyManager, character.CountryProvider, etinMock.Object, serviceProvider.Object);
            guildManager.SetGuildInfo(1, "", 1);

            var result = await guildManager.TryBuyHouse();

            Assert.Equal(GuildHouseBuyReason.AlreadyBought, result);
        }

        [Fact]
        [Description("Player can buy guild house.")]
        public async void TryBuyHouse_SuccessTest()
        {
            var character = CreateCharacter(testMap);

            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.FindAsync(It.IsAny<uint>())).ReturnsAsync(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 1, HasHouse = false });
            database.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character.InventoryManager, character.PartyManager, character.CountryProvider, etinMock.Object, serviceProvider.Object);
            guildManager.SetGuildInfo(1, "", 1);

            var result = await guildManager.TryBuyHouse();

            Assert.Equal(GuildHouseBuyReason.Ok, result);
        }

        [Fact]
        [Description("Guild house NPC can be used only if guild has right rank.")]
        public void CanUseNpc_RequiredRankTest()
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.Find(It.IsAny<int>())).Returns(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 10 });

            var houseConfig = new GuildHouseConfiguration()
            {
                NpcInfos = new List<GuildHouseNpcInfo>()
                {
                    new GuildHouseNpcInfo()
                    {
                        NpcType = NpcType.Merchant,
                        LightNpcTypeId = 1,
                        MinRank = 5
                    }
                }
            };

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), houseConfig, gameWorldMock.Object, timeMock.Object, inventoryMock.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceMock.Object);
            guildManager.SetGuildInfo(1, "", 6);

            var canUse = guildManager.CanUseNpc(NpcType.Merchant, 1, out var requiredRank);

            Assert.False(canUse);
            Assert.Equal(5, requiredRank);
        }

        [Fact]
        [Description("Guild house NPC can be used only if guild has rank <= 30.")]
        public void CanUseNpc_Rank31Test()
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.Find(It.IsAny<int>())).Returns(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 31 });

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var houseConfig = new GuildHouseConfiguration();

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), houseConfig, gameWorldMock.Object, timeMock.Object, inventoryMock.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceProvider.Object);
            guildManager.Init(1, 1, "", 1, 31, new DbCharacter[0]);

            var canUse = guildManager.CanUseNpc(NpcType.Merchant, 1, out var requiredRank);

            Assert.False(canUse);
            Assert.Equal(30, requiredRank);
        }

        [Fact]
        [Description("Guild house NPC can be used only if guild has right rank.")]
        public void CanUseNpc_SuccessTest()
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.Find(It.IsAny<int>())).Returns(new DbGuild("test_guild", "test_message", 99, Fraction.Light) { Rank = 5 });

            var houseConfig = new GuildHouseConfiguration()
            {
                NpcInfos = new List<GuildHouseNpcInfo>()
                {
                    new GuildHouseNpcInfo()
                    {
                        NpcType = NpcType.Merchant,
                        LightNpcTypeId = 1,
                        MinRank = 5
                    }
                }
            };

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), houseConfig, gameWorldMock.Object, timeMock.Object, inventoryMock.Object, partyMock.Object, countryMock.Object, etinMock.Object, serviceMock.Object);
            guildManager.SetGuildInfo(1, "", 5);

            var canUse = guildManager.CanUseNpc(NpcType.Merchant, 1, out var requiredRank);

            Assert.True(canUse);
        }

        [Fact]
        [Description("It should be possible to return etin.")]
        public async void ReturnEtinTest()
        {
            var character = CreateCharacter(testMap);;
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Etin_100.Type, Etin_100.TypeId), "");
            Assert.NotEmpty(character.InventoryManager.InventoryItems);

            var guild = new DbGuild("test_guild", "test_message", 1, Fraction.Light) { Etin = 50 };
            var database = new Mock<IDatabase>();
            database.Setup(x => x.Guilds.FindAsync(It.IsAny<uint>())).ReturnsAsync(guild);
            database.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IDatabase)))
                            .Returns(database.Object);

            var etin = new Mock<IEtinManager>();
            etin.Setup(x => x.GetEtin(It.IsAny<uint>())).ReturnsAsync(50);

            var guildManager = new GuildManager(guildLoggerMock.Object, new GuildConfiguration(), new GuildHouseConfiguration(), gameWorldMock.Object, timeMock.Object, character.InventoryManager, partyMock.Object, countryMock.Object, etin.Object, serviceProvider.Object);
            guildManager.SetGuildInfo(1, "", 9);

            await guildManager.ReturnEtin();

            Assert.Empty(character.InventoryManager.InventoryItems);
            Assert.Equal(150, guild.Etin);
        }
    }
}
