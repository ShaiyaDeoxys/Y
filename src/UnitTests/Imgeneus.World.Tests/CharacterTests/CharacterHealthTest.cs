using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Tests;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.Game.Tests.CharacterTests
{
    public class CharacterHealthTest : BaseTest
    {
        [Fact]
        [Description("Current HP can not be more that max HP.")]
        public void CurrentHPNotMoreThanMaxHPTest()
        {
            var character = CreateCharacter(testMap);
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);

            character.HealthManager.FullRecover();
            Assert.Equal(2050, character.HealthManager.CurrentHP);
            Assert.Equal(250, character.HealthManager.CurrentMP);
            Assert.Equal(1050, character.HealthManager.CurrentSP);

            character.InventoryManager.MoveItem(0, 1, 1, 0); // Take off item.
            Assert.Equal(100, character.HealthManager.CurrentHP);
            Assert.Equal(200, character.HealthManager.CurrentMP);
            Assert.Equal(300, character.HealthManager.CurrentSP);
        }
    }
}
