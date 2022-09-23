using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Zone;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.PartyTests
{
    public class RaidTest : BaseTest
    {
        private Map _map;

        public RaidTest()
        {
            _map = testMap;
        }

        [Fact]
        [Description("First player, that connected raid is its' leader. Second - subleader.")]
        public void Raid_Leader()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);
            Assert.False(character1.PartyManager.IsPartyLead);

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;

            Assert.True(character1.PartyManager.IsPartyLead);
            Assert.Equal(character1, raid.Leader);

            Assert.True(character2.PartyManager.IsPartySubLeader);
            Assert.Equal(character2, raid.SubLeader);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Leader, then leader should get all items in drop.")]
        public void Raid_DropToLeader()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            var raid = new Raid(true, RaidDropType.Leader, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;

            raid.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.Equal(3, character1.InventoryManager.InventoryItems.Count);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Leader, but leader is too far away, then he doesn't get drop.")]
        public void Raid_DropToLeader_LeaderIsFarAway()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            // Set leader far away from character2.
            character1.MovementManager.PosX = 1000;
            character1.MovementManager.PosY = 1000;
            character1.MovementManager.PosZ = 1000;

            var raid = new Raid(true, RaidDropType.Leader, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;

            raid.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.Empty(character1.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Group, items are distributed one by one to each raid member.")]
        public void Raid_DropToGroup()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;

            raid.DistributeDrop(new List<Item>()
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
        [Description("If drop type is RaidDropType.Group, but some member is far away he doesn't get drop.")]
        public void Raid_DropToGroup_FarAway()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);
            var character3 = CreateCharacter(_map);

            // Set character3 far away from character1 and character2.
            character3.MovementManager.PosX = 1000;
            character3.MovementManager.PosY = 1000;
            character3.MovementManager.PosZ = 1000;

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;
            character3.PartyManager.Party = raid;

            raid.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.Equal(2, character1.InventoryManager.InventoryItems.Count);
            Assert.Single(character2.InventoryManager.InventoryItems);
            Assert.Empty(character3.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Group, money should be distributed equally.")]
        public void Raid_DropToGroup_GoldDistribution()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);
            var character3 = CreateCharacter(_map);

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;
            character3.PartyManager.Party = raid;

            var money = new Item(100);
            raid.DistributeDrop(new List<Item>() { money }, character2);

            Assert.Equal((uint)33, character1.InventoryManager.Gold);
            Assert.Equal((uint)33, character2.InventoryManager.Gold);
            Assert.Equal((uint)33, character3.InventoryManager.Gold);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Group, but members do not have place in inventory, items are not distributed.")]
        public void Raid_DropToGroup_NoPlaceInInventory()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);

            for (int i = 0; i < 5 * 25; i++) // 5 bags, 24 slots per 1 bag.
            {
                character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId), "");
                character2.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId), "");
            }

            var raid = new Raid(true, RaidDropType.Group, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;

            var notDistributedItems = raid.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.Equal(3, notDistributedItems.Count);
        }

        [Fact]
        [Description("If drop type is RaidDropType.Random, drop items should be assign to random users.")]
        public void Raid_DropToRandom()
        {
            var character1 = CreateCharacter(_map);
            var character2 = CreateCharacter(_map);
            var character3 = CreateCharacter(testMap);

            var raid = new Raid(true, RaidDropType.Random, packetFactoryMock.Object);
            character1.PartyManager.Party = raid;
            character2.PartyManager.Party = raid;
            character3.PartyManager.Party = raid;

            raid.DistributeDrop(new List<Item>()
            {
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId),
                new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId)
            }, character2);

            Assert.True(character1.InventoryManager.InventoryItems.Count >= 0);
            Assert.True(character2.InventoryManager.InventoryItems.Count >= 0);
            Assert.True(character3.InventoryManager.InventoryItems.Count >= 0);

            Assert.Equal(3, character1.InventoryManager.InventoryItems.Count + character2.InventoryManager.InventoryItems.Count + character3.InventoryManager.InventoryItems.Count);
        }
    }
}
