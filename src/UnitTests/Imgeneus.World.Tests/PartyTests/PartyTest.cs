using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Zone;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.PartyTests
{
    public class PartyTest : BaseTest
    {
        private Map _map;

        public PartyTest()
        {
            _map = testMap;
        }

        [Fact]
        [Description("First player, that connected party is its' leader.")]
        public void Party_Leader()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            Assert.False(character1.PartyManager.IsPartyLead);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            Assert.True(character1.PartyManager.IsPartyLead);
            Assert.Equal(character1, party.Leader);
        }

        [Fact]
        [Description("Party drop should be for each player, 1 by 1.")]
        public void Party_DropCalculation()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            party.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.Equal(2, character1.InventoryManager.InventoryItems.Count);
            Assert.Single(character2.InventoryManager.InventoryItems);

            Assert.Equal(WaterArmor.Type, character1.InventoryManager.InventoryItems[(1, 0)].Type);
            Assert.Equal(FireSword.Type, character1.InventoryManager.InventoryItems[(1, 1)].Type);

            Assert.Equal(WaterArmor.Type, character2.InventoryManager.InventoryItems[(1, 0)].Type);
        }

        [Fact]
        [Description("Experience should be split among party members who are within range of mob killer player.")]
        public void Party_Experience()
        {
            var killerCharacter = CreateCharacter(_map);
            var nearbyCharacter = CreateCharacter(_map);
            var farAwayCharacter = CreateCharacter(_map);

            killerCharacter.AdditionalInfoManager.Grow = Mode.Ultimate;
            nearbyCharacter.AdditionalInfoManager.Grow = Mode.Ultimate;
            farAwayCharacter.AdditionalInfoManager.Grow = Mode.Ultimate;

            killerCharacter.MovementManager.PosX = 0;
            killerCharacter.MovementManager.PosZ = 0;

            nearbyCharacter.MovementManager.PosX = 0;
            nearbyCharacter.MovementManager.PosZ = 0;

            farAwayCharacter.MovementManager.PosX = 1000;
            farAwayCharacter.MovementManager.PosZ = 1000;

            var party = new Party(packetFactoryMock.Object);
            killerCharacter.PartyManager.Party = party;
            nearbyCharacter.PartyManager.Party = party;
            farAwayCharacter.PartyManager.Party = party;

            var mob = CreateMob(Wolf.Id, _map);

            killerCharacter.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            nearbyCharacter.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            farAwayCharacter.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);

            Assert.Equal((uint)3022800, killerCharacter.LevelingManager.Exp);
            Assert.Equal((uint)3022800, nearbyCharacter.LevelingManager.Exp);
            Assert.Equal((uint)3022800, farAwayCharacter.LevelingManager.Exp);

            mob.HealthManager.DecreaseHP(20000000, killerCharacter);

            Assert.True(mob.HealthManager.IsDead);

            var expectedNewExp = (uint)3022800 + 120 / 3;

            Assert.Equal(expectedNewExp, killerCharacter.LevelingManager.Exp);
            Assert.Equal(expectedNewExp, nearbyCharacter.LevelingManager.Exp);
            Assert.Equal((uint)3022800, farAwayCharacter.LevelingManager.Exp);
        }

        [Fact]
        [Description("Experience in perfect parties should be gained as if there were only 2 party members.")]
        public void Party_PerfectPartyExperience()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);
            var character3 = CreateCharacter(_map);
            var character4 = CreateCharacter(_map);
            var character5 = CreateCharacter(_map);
            var character6 = CreateCharacter(_map);
            var character7 = CreateCharacter(_map);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;
            character3.PartyManager.Party = party;
            character4.PartyManager.Party = party;
            character5.PartyManager.Party = party;
            character6.PartyManager.Party = party;
            character7.PartyManager.Party = party;

            var mob = CreateMob(Wolf.Id, _map);

            character1.AdditionalInfoManager.Grow = Mode.Ultimate;
            character2.AdditionalInfoManager.Grow = Mode.Ultimate;
            character3.AdditionalInfoManager.Grow = Mode.Ultimate;
            character4.AdditionalInfoManager.Grow = Mode.Ultimate;
            character5.AdditionalInfoManager.Grow = Mode.Ultimate;
            character6.AdditionalInfoManager.Grow = Mode.Ultimate;
            character7.AdditionalInfoManager.Grow = Mode.Ultimate;

            character1.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character2.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character3.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character4.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character5.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character6.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);
            character7.LevelingManager.TryChangeLevel(mob.LevelProvider.Level);

            Assert.Equal((uint)3022800, character1.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character2.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character3.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character4.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character5.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character6.LevelingManager.Exp);
            Assert.Equal((uint)3022800, character7.LevelingManager.Exp);

            mob.HealthManager.DecreaseHP(20000000, character1);

            Assert.True(mob.HealthManager.IsDead);

            var expectedExp = (uint)3022800 + 120 / 2;

            Assert.Equal(expectedExp, character1.LevelingManager.Exp);
            Assert.Equal(expectedExp, character2.LevelingManager.Exp);
            Assert.Equal(expectedExp, character3.LevelingManager.Exp);
            Assert.Equal(expectedExp, character4.LevelingManager.Exp);
            Assert.Equal(expectedExp, character5.LevelingManager.Exp);
            Assert.Equal(expectedExp, character6.LevelingManager.Exp);
            Assert.Equal(expectedExp, character7.LevelingManager.Exp);
        }
    }
}
