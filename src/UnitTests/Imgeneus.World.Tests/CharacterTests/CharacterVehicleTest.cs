using System.ComponentModel;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterVehicleTest : BaseTest
    {
        [Fact]
        [Description("Character's movement speed should increase while using a mount.")]
        public void CharacterMountTest()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, HorseSummonStone.Type, HorseSummonStone.TypeId), "");
            character.InventoryManager.Mount = character.InventoryManager.InventoryItems[(1, 0)];

            character.VehicleManager.CallVehicle(true);
            Assert.Equal(MoveSpeed.VeryFast, character.SpeedManager.TotalMoveSpeed);

            character.VehicleManager.RemoveVehicle();
            Assert.Equal(MoveSpeed.Normal, character.SpeedManager.TotalMoveSpeed);
        }

        [Fact]
        [Description("Character's mount should be removed if equipped mount is changed while still using the previous one.")]
        public void CharacterMountChangeTest()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, HorseSummonStone.Type, HorseSummonStone.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, HorseSummonStone.Type, HorseSummonStone.TypeId), "");

            // Equip mount 1
            character.InventoryManager.Mount = character.InventoryManager.InventoryItems[(1, 0)];
            character.VehicleManager.CallVehicle(true);
            Assert.Equal(MoveSpeed.VeryFast, character.SpeedManager.TotalMoveSpeed);

            // Equip mount 2
            character.InventoryManager.Mount = character.InventoryManager.InventoryItems[(1, 1)];
            Assert.Equal(MoveSpeed.Normal, character.SpeedManager.TotalMoveSpeed);
        }
    }
}
