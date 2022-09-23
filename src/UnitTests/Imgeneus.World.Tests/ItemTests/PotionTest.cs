using Imgeneus.World.Game.Inventory;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.ItemTests
{
    public class PotionTest : BaseTest
    {
        [Fact]
        [Description("Etain Potion should recover 75% of hp, mp, sp.")]
        public void EtainPotionTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter();

            Assert.Equal(100, character.HealthManager.MaxHP);
            Assert.Equal(200, character.HealthManager.MaxMP);
            Assert.Equal(300, character.HealthManager.MaxSP);

            character.HealthManager.IncreaseHP(100);
            Assert.Equal(100, character.HealthManager.CurrentHP);
            Assert.Equal(0, character.HealthManager.CurrentMP);
            Assert.Equal(0, character.HealthManager.CurrentSP);

            character.HealthManager.DecreaseHP(90, character2);
            Assert.Equal(10, character.HealthManager.CurrentHP);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, EtainPotion.Type, EtainPotion.TypeId), "");
            character.InventoryManager.TryUseItem(1, 0);

            Assert.Equal(85, character.HealthManager.CurrentHP);
            Assert.Equal(150, character.HealthManager.CurrentMP);
            Assert.Equal(225, character.HealthManager.CurrentSP);
        }

        [Fact]
        [Description("Red apple should recover 50 hp.")]
        public void RedAppleTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter();

            Assert.Equal(100, character.HealthManager.MaxHP);
            Assert.Equal(200, character.HealthManager.MaxMP);
            Assert.Equal(300, character.HealthManager.MaxSP);

            character.HealthManager.IncreaseHP(100);
            Assert.Equal(100, character.HealthManager.CurrentHP);
            Assert.Equal(0, character.HealthManager.CurrentMP);
            Assert.Equal(0, character.HealthManager.CurrentSP);

            character.HealthManager.DecreaseHP(90, character2);
            Assert.Equal(10, character.HealthManager.CurrentHP);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.InventoryManager.TryUseItem(1, 0);

            Assert.Equal(60, character.HealthManager.CurrentHP);
        }

        [Fact]
        [Description("Apple should have 15 sec cooldown. If red apple is used, green apple can not be use.")]
        public void RedAndGreenAppleTest()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, GreenApple.Type, GreenApple.TypeId), "");

            // Can use green apple
            Assert.True(character.InventoryManager.CanUseItem(character.InventoryManager.InventoryItems[(1, 1)]));

            // Used red apple.
            character.InventoryManager.TryUseItem(1, 0);
            Assert.False(character.InventoryManager.CanUseItem(character.InventoryManager.InventoryItems[(1, 1)]));

            // Green apple is still in inventory.
            character.InventoryManager.TryUseItem(1, 1);
            Assert.NotNull(character.InventoryManager.InventoryItems[(1, 1)]);
        }
    }
}
