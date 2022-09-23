using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Warehouse;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.WarehouseTests
{
    public class WarehouseTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to put item into warehouse.")]
        public void ShouldBePossibleToPutItemIntoWarehouse()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");
            Assert.True(character.InventoryManager.InventoryItems.ContainsKey((1, 0)));

            character.InventoryManager.MoveItem(1, 0, WarehouseManager.WAREHOUSE_BAG, 0);
            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 0)));
            Assert.True(character.WarehouseManager.Items.ContainsKey(0));
        }

        [Fact]
        [Description("Fee 5% should be taken, when item takinen out from warehouse.")]
        public void FeeWhenTakeOutFromWarehouse()
        {
            var character = CreateCharacter();
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, WarehouseManager.WAREHOUSE_BAG, 0);
            Assert.True(character.WarehouseManager.Items.ContainsKey(0));

            character.InventoryManager.MoveItem(WarehouseManager.WAREHOUSE_BAG, 0, 1, 0);
            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 0)));
            Assert.True(character.WarehouseManager.Items.ContainsKey(0));

            character.InventoryManager.Gold = 100;
            character.InventoryManager.MoveItem(WarehouseManager.WAREHOUSE_BAG, 0, 1, 0);
            Assert.True(character.InventoryManager.InventoryItems.ContainsKey((1, 0)));
            Assert.False(character.WarehouseManager.Items.ContainsKey(0));
            Assert.Equal((uint)95, character.InventoryManager.Gold);
        }

        [Fact]
        [Description("Item can be put into double warehouse, only if double-warehouse was used.")]
        public void DoubleWarehouseCanBeUsed()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, WarehouseManager.WAREHOUSE_BAG, 120);
            Assert.True(character.InventoryManager.InventoryItems.ContainsKey((1, 0)));
            Assert.False(character.WarehouseManager.Items.ContainsKey(0));

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, DoubleWarehouse.Type, DoubleWarehouse.TypeId), "");
            Assert.True(character.InventoryManager.InventoryItems.ContainsKey((1, 1)));
            character.InventoryManager.TryUseItem(1, 1);

            Assert.NotEmpty(character.BuffsManager.ActiveBuffs);

            character.InventoryManager.MoveItem(1, 0, WarehouseManager.WAREHOUSE_BAG, 120);
            Assert.True(character.WarehouseManager.Items.ContainsKey(120));
        }
    }
}
