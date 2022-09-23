using Imgeneus.Game.Monster;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.ShopTests
{
    public class ShopTest : BaseTest
    {
        private Map _capital => new Map(
                    35,
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

        [Fact]
        [Description("Local shop can be opened only on special maps: both capitals and black market.")]
        public void ShopCanBeOpenedInSpecialMap()
        {
            var character = CreateCharacter();
            var ok = character.ShopManager.TryBegin();
            Assert.False(ok);

            character.MapProvider.Map = _capital;

            ok = character.ShopManager.TryBegin();
            Assert.True(ok);
        }

        [Fact]
        [Description("Item from inventory can be added to the local shop.")]
        public void ItemCanBeAddedToShop()
        {
            var character = CreateCharacter(_capital);
            var ok = character.ShopManager.TryAddItem(1, 0, 0, 100);
            Assert.False(ok);
            Assert.Empty(character.ShopManager.Items);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");

            ok = character.ShopManager.TryAddItem(1, 0, 0, 100);
            Assert.True(ok);
            Assert.NotEmpty(character.ShopManager.Items);
            Assert.True(character.ShopManager.Items[0].IsInShop);
            Assert.Equal((uint)100, character.ShopManager.Items[0].ShopPrice);
        }

        [Fact]
        [Description("Item from inventory can be added to the local shop only once.")]
        public void ItemCanBeAddedToShopOnlyOnce()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            var ok = character.ShopManager.TryAddItem(1, 0, 0, 100);
            Assert.True(ok);

            ok = character.ShopManager.TryAddItem(1, 0, 1, 100);
            Assert.False(ok);
        }

        [Fact]
        [Description("Item should have unique slot in local shop.")]
        public void ItemShouldHaveUniqueSlot()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, GreenApple.Type, GreenApple.TypeId), "");

            var ok = character.ShopManager.TryAddItem(1, 0, 0, 100);
            Assert.True(ok);

            ok = character.ShopManager.TryAddItem(1, 1, 0, 100);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should be possible to remove item from local shop and add it again.")]
        public void ItemRemove()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");

            var ok = character.ShopManager.TryAddItem(1, 0, 0, 100);
            Assert.True(ok);

            ok = character.ShopManager.TryRemoveItem(0);
            Assert.True(ok);
            Assert.Empty(character.ShopManager.Items);
            Assert.False(character.InventoryManager.InventoryItems[(1, 0)].IsInShop);
            Assert.Equal((uint)0, character.InventoryManager.InventoryItems[(1, 0)].ShopPrice);

            ok = character.ShopManager.TryRemoveItem(0);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should be possible to start local shop.")]
        public void ShopStart()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.ShopManager.TryAddItem(1, 0, 0, 100);

            var ok = character.ShopManager.TryStart("my shop");
            Assert.True(ok);
            Assert.True(character.ShopManager.IsShopOpened);
        }

        [Fact]
        [Description("The local shop won't start if name is incorrect or no items.")]
        public void ShopWontStart()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");

            var ok = character.ShopManager.TryStart(string.Empty);
            Assert.False(ok);

            ok = character.ShopManager.TryStart("my shop");
            Assert.False(ok);

            character.ShopManager.TryAddItem(1, 0, 0, 100);
            ok = character.ShopManager.TryStart("my shop");

            Assert.True(ok);
        }

        [Fact]
        [Description("It should not be possible to start local shop twice.")]
        public void ShopStartTwice()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.ShopManager.TryAddItem(1, 0, 0, 100);

            var ok = character.ShopManager.TryStart("my shop");
            Assert.True(ok);

            ok = character.ShopManager.TryStart("my shop");
            Assert.False(ok);
        }

        [Fact]
        [Description("It should be possible to cancel local shop.")]
        public void ShopCancel()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.ShopManager.TryAddItem(1, 0, 0, 100);
            character.ShopManager.TryStart("my shop");
            Assert.True(character.ShopManager.IsShopOpened);

            var ok = character.ShopManager.TryCancel();
            Assert.True(ok);
            Assert.False(character.ShopManager.IsShopOpened);
        }

        [Fact]
        [Description("It should not be possible to cancel local shop twice.")]
        public void ShopCancelTwice()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.ShopManager.TryAddItem(1, 0, 0, 100);
            character.ShopManager.TryStart("my shop");
            Assert.True(character.ShopManager.IsShopOpened);

            var ok = character.ShopManager.TryCancel();
            ok = character.ShopManager.TryCancel();
            Assert.False(ok);
            Assert.False(character.ShopManager.IsShopOpened);
        }

        [Fact]
        [Description("It should be possible to end local shop.")]
        public void ShopEnd()
        {
            var character = CreateCharacter(_capital);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.ShopManager.TryAddItem(1, 0, 0, 100);
            character.ShopManager.TryStart("my shop");
            Assert.NotEmpty(character.ShopManager.Items);
            Assert.True(character.ShopManager.IsShopOpened);

            var ok = character.ShopManager.TryEnd();
            Assert.True(ok);
            Assert.Empty(character.ShopManager.Items);
            Assert.False(character.ShopManager.IsShopOpened);
        }

        [Fact]
        [Description("It should be not possible to buy item from local shop, is local shop is not selected.")]
        public void ShopBuyInvalidShop()
        {
            var character = CreateCharacter(_capital);
            var ok = character.ShopManager.TryBuyItem(1, 1, out var soldItem, out var shopItem);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should be possible to buy item from local shop, if item is presented there.")]
        public void ShopBuyInvalidSlot()
        {
            var character1 = CreateCharacter(_capital);
            var character2 = CreateCharacter(_capital);

            character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character1.ShopManager.TryAddItem(1, 0, 0, 100);
            character1.ShopManager.TryStart("my shop");

            character2.ShopManager.UseShop = character1.ShopManager;
            var ok = character2.ShopManager.TryBuyItem(1, 1, out var soldItem, out var shopItem);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should not be possible to buy item from local shop, if not enough money.")]
        public void ShopBuyNotEnoughMoney()
        {
            var character1 = CreateCharacter(_capital);
            var character2 = CreateCharacter(_capital);

            character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character1.ShopManager.TryAddItem(1, 0, 0, 100);
            character1.ShopManager.TryStart("my shop");

            character2.ShopManager.UseShop = character1.ShopManager;
            var ok = character2.ShopManager.TryBuyItem(0, 1, out var soldItem, out var shopItem);
            Assert.False(ok);
        }

        [Fact]
        [Description("It should not be possible to buy more items from local shop than there presented.")]
        public void ShopBuyRightCount()
        {
            var character1 = CreateCharacter(_capital);
            var character2 = CreateCharacter(_capital);

            character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId, 10), "");
            character1.ShopManager.TryAddItem(1, 0, 0, 100);
            character1.ShopManager.TryStart("my shop");

            character2.InventoryManager.Gold = 10 * 100; // 10 apples * 100 gold
            character2.ShopManager.UseShop = character1.ShopManager;
            var ok = character2.ShopManager.TryBuyItem(0, 11, out var soldItem, out var shopItem); // Try to buy 11 apples
            Assert.True(ok);

            Assert.Empty(character1.InventoryManager.InventoryItems);
            Assert.Equal((uint)10 * 100, character1.InventoryManager.Gold);

            Assert.NotEmpty(character2.InventoryManager.InventoryItems);
            Assert.Equal(10, character2.InventoryManager.InventoryItems[(1, 0)].Count);
            Assert.Equal((uint)0, character2.InventoryManager.Gold);
        }

        [Fact]
        [Description("It should be possible to buy half of the items.")]
        public void ShopBuyRightCount2()
        {
            var character1 = CreateCharacter(_capital);
            var character2 = CreateCharacter(_capital);

            character1.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId, 10), "");
            character1.ShopManager.TryAddItem(1, 0, 0, 100);
            character1.ShopManager.TryStart("my shop");

            character2.InventoryManager.Gold = 10 * 100; // 10 apples * 100 gold
            character2.ShopManager.UseShop = character1.ShopManager;

            var ok = character2.ShopManager.TryBuyItem(0, 5, out var soldItem, out var shopItem); // Try to buy 5 apples
            Assert.True(ok);

            Assert.NotEmpty(character1.InventoryManager.InventoryItems);
            Assert.Equal(5, character1.InventoryManager.InventoryItems[(1, 0)].Count);
            Assert.Equal((uint)5 * 100, character1.InventoryManager.Gold);

            Assert.NotEmpty(character2.InventoryManager.InventoryItems);
            Assert.Equal(5, character2.InventoryManager.InventoryItems[(1, 0)].Count);
            Assert.Equal((uint)5 * 100, character2.InventoryManager.Gold);
        }
    }
}
