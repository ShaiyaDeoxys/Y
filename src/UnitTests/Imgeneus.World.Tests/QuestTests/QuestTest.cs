using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.QuestTests
{
    public class QuestTest : BaseTest
    {
        [Fact]
        [Description("Quests can be initialized from database.")]
        public void QuestsManagerCanBeInitFromDatabase()
        {
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, new Mock<IInventoryManager>().Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = NewBeginnings.Id, Finish = true, Success = true });
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = Bartering.Id });

            questsManager.Init(1, dbQuests);

            Assert.Equal(2, questsManager.Quests.Count);
        }

        [Fact]
        [Description("Quests can be initialized from database, regardless if quest is preloaded.")]
        public void QuestNotPreloadedShouldNotBreakInitiation()
        {
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, new Mock<IInventoryManager>().Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = 999, Finish = true, Success = true });

            questsManager.Init(1, dbQuests);

            Assert.Empty(questsManager.Quests);
        }

        [Fact]
        [Description("It should be possibl to quit quest.")]
        public void QuitQuest()
        {
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, new Mock<IInventoryManager>().Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = NewBeginnings.Id });

            questsManager.Init(1, dbQuests);

            Assert.Single(questsManager.Quests);
            Assert.False(questsManager.Quests[0].IsFinished);
            Assert.False(questsManager.Quests[0].IsSuccessful);

            questsManager.QuitQuest(NewBeginnings.Id);
            Assert.True(questsManager.Quests[0].IsFinished);
            Assert.False(questsManager.Quests[0].IsSuccessful);
        }

        [Fact]
        [Description("Finish non-existing quest should not break app.")]
        public void FinishNonExistingQuest()
        {
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, new Mock<IInventoryManager>().Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var ok = questsManager.TryFinishQuest(0, 1, out var q);

            Assert.False(ok);
        }

        [Fact]
        [Description("It should not be possible to finish quest, that has been already finished.")]
        public void FinishAlreadyFinishedQuest()
        {
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, new Mock<IInventoryManager>().Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = NewBeginnings.Id, Finish = true, Success = true });

            questsManager.Init(1, dbQuests);

            var ok = questsManager.TryFinishQuest(0, NewBeginnings.Id, out var q);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should not be possible to finish quest, until mob count is reached.")]
        public void MobCountIsCheckedQuest()
        {
            var inventoryManager = new Mock<IInventoryManager>();
            inventoryManager.SetupGet(x => x.InventoryItems)
                            .Returns(new ConcurrentDictionary<(byte Bag, byte Slot), Item>());

            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, inventoryManager.Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = NewBeginnings.Id });

            questsManager.Init(1, dbQuests);

            var ok = questsManager.TryFinishQuest(0, NewBeginnings.Id, out var q);
            Assert.False(ok);

            questsManager.UpdateQuestMobCount(NewBeginnings.RequiredMobId1);
            ok = questsManager.TryFinishQuest(0, NewBeginnings.Id, out q);
            Assert.False(ok);

            questsManager.UpdateQuestMobCount(NewBeginnings.RequiredMobId1);
            questsManager.UpdateQuestMobCount(NewBeginnings.RequiredMobId1);
            questsManager.UpdateQuestMobCount(NewBeginnings.RequiredMobId1);
            questsManager.UpdateQuestMobCount(NewBeginnings.RequiredMobId1);

            ok = questsManager.TryFinishQuest(0, NewBeginnings.Id, out q);
            Assert.True(ok);
        }

        [Fact]
        [Description("It should not be possible to finish quest, until farm items count is reached.")]
        public void FarmItemsAreCheckedQuest()
        {
            var items = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();
            var inventoryManager = new Mock<IInventoryManager>();
            inventoryManager.SetupGet(x => x.InventoryItems)
                            .Returns(items);

            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, inventoryManager.Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = Bartering.Id });

            questsManager.Init(1, dbQuests);

            var ok = questsManager.TryFinishQuest(0, Bartering.Id, out var q);
            Assert.False(ok);

            items.TryAdd((1, 0), new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Bartering.FarmItems[0].Type, Bartering.FarmItems[0].TypeId, Bartering.FarmItems[0].Count));
            ok = questsManager.TryFinishQuest(0, Bartering.Id, out q);
            Assert.True(ok);
        }

        [Fact]
        [Description("It should be possible to finish quest and select among different options.")]
        public void PlayerShouldBeAbleToSelectQuestResult()
        {
            var inventoryManager = new Mock<IInventoryManager>();
            var questsManager = new QuestsManager(new Mock<ILogger<QuestsManager>>().Object, definitionsPreloader.Object, new Mock<IMapProvider>().Object, gameWorldMock.Object, databaseMock.Object, new Mock<IPartyManager>().Object, inventoryManager.Object, enchantConfig.Object, itemCreateConfig.Object, new Mock<ILevelingManager>().Object);
            var dbQuests = new List<DbCharacterQuest>();
            dbQuests.Add(new DbCharacterQuest() { CharacterId = 1, QuestId = SkillsAndStats.Id });

            questsManager.Init(1, dbQuests);

            var ok = questsManager.TryFinishQuestSelect(0, SkillsAndStats.Id, 0);
            Assert.True(ok);

            inventoryManager.Verify(x => x.AddItem(It.IsAny<Item>(), "quest_finish", false), Times.Once());
        }
    }
}
