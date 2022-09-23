using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.SummonTests
{
    public class SummonPartyTest : BaseTest
    {
        [Fact]
        [Description("It should not throw exception if character is not in party.")]
        public void DoNotSummonWithNoParty()
        {
            var character = CreateCharacter();
            character.PartyManager.SummonMembers();
            character.PartyManager.SetSummonAnswer(true);
        }

        [Fact]
        [Description("It should not use summon item if character is not in party.")]
        public void DoNotUseSummonItemWithNoParty()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PartySummonRune.Type, PartySummonRune.TypeId), "");
            Assert.Single(character.InventoryManager.InventoryItems);

            character.InventoryManager.TryUseItem(1, 0);
            Assert.Single(character.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("It should be created SummonRequest.")]
        public void SummonRequestShouldBeCreated()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            Assert.Null(character1.PartyManager.Party.SummonRequest);
            Assert.Null(character2.PartyManager.Party.SummonRequest);

            character1.PartyManager.SummonMembers();

            Assert.Equal(character1.PartyManager.Party.SummonRequest, character2.PartyManager.Party.SummonRequest);
            Assert.NotNull(character1.PartyManager.Party.SummonRequest);
            Assert.Equal((uint)1, character1.PartyManager.Party.SummonRequest.OwnerId);
            Assert.True(character1.PartyManager.IsSummoning);
        }

        [Fact]
        [Description("SummonRequest should contain party members.")]
        public void SummonRequestShouldContainPartyMembers()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            character1.PartyManager.SummonMembers(true);

            Assert.NotEmpty(character1.PartyManager.Party.SummonRequest.MemberAnswers);
            Assert.Null(character1.PartyManager.Party.SummonRequest.MemberAnswers[character2.Id]);
        }

        [Fact]
        [Description("Summon is canceled if character gets damage.")]
        public void SummonIsCanceledIfGotDamage()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);
            var enemy = CreateCharacter(map, country: Fraction.Dark);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            character1.PartyManager.SummonMembers();
            Assert.True(character1.PartyManager.IsSummoning);

            character1.HealthManager.DecreaseHP(1, enemy);
            Assert.False(character1.PartyManager.IsSummoning);
        }

        [Fact]
        [Description("Summon item should be used.")]
        public void SummonItemShouldBeUsed()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var summonItem = character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PartySummonRune.Type, PartySummonRune.TypeId), "");
            Assert.Single(character1.InventoryManager.InventoryItems);

            character1.PartyManager.SummonMembers(true, summonItem);
            Assert.Empty(character1.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("Summon request is cleaned, when all mebers answered.")]
        public void SummonRequestIsCleanedWhenAllAnswered()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);
            var character3 = CreateCharacter(map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;
            character3.PartyManager.Party = party;

            character1.PartyManager.SummonMembers(true);

            Assert.NotNull(character1.PartyManager.Party.SummonRequest);
            Assert.Equal(2, character1.PartyManager.Party.SummonRequest.MemberAnswers.Count);
            Assert.Null(character1.PartyManager.Party.SummonRequest.MemberAnswers[character2.Id]);
            Assert.Null(character1.PartyManager.Party.SummonRequest.MemberAnswers[character3.Id]);

            character2.PartyManager.SetSummonAnswer(false);
            Assert.NotNull(character1.PartyManager.Party.SummonRequest);
            Assert.False(character1.PartyManager.Party.SummonRequest.MemberAnswers[character2.Id]);

            character3.PartyManager.SetSummonAnswer(false);
            Assert.Null(character1.PartyManager.Party.SummonRequest);
        }

        [Fact]
        [Description("Summonraid should summon only 5 raid members.")]
        public void SummonRaid()
        {
            var map = testMap;
            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);
            var character3 = CreateCharacter(map);

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;
            character3.PartyManager.Party = raid;

            Assert.Equal(2, raid.GetIndex(character3));

            raid.MoveCharacter(2, 6); // Move to the second group
            Assert.Equal(6, raid.GetIndex(character3));

            character1.PartyManager.SummonMembers(true);
            Assert.Single(character1.PartyManager.Party.SummonRequest.MemberAnswers);
            Assert.Null(character1.PartyManager.Party.SummonRequest.MemberAnswers[character2.Id]);
            Assert.False(character1.PartyManager.Party.SummonRequest.MemberAnswers.ContainsKey(character3.Id));
        }
    }
}
