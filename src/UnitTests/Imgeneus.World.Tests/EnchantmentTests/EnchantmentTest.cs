using Imgeneus.World.Game.Inventory;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.EnchantmentTests
{
    public class EnchantmentTest : BaseTest
    {
        [Fact]
        [Description("Enchantment rate is 0 if item has already enchantment level 20.")]
        public void EnchantmentShouldBe0IfLevelIs20()
        {
            var character = CreateCharacter();

            var item = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            item.EnchantmentLevel = 20;
            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, AssaultLapisia.Type, AssaultLapisia.TypeId);

            Assert.Equal(0, character.LinkingManager.GetEnchantmentRate(item, lapisia, null));
        }

        [Fact]
        [Description("Enchantment rate is calculated right.")]
        public void EnchantmentRateShouldBeCalculatedRight()
        {
            var character = CreateCharacter();

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            armor.EnchantmentLevel = 19;

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            weapon.EnchantmentLevel = 19;

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, AssaultLapisia.Type, AssaultLapisia.TypeId);

            Assert.Equal(200, character.LinkingManager.GetEnchantmentRate(armor, lapisia, null));
            Assert.Equal(200, character.LinkingManager.GetEnchantmentRate(weapon, lapisia, null));
        }

        [Fact]
        [Description("Enchantment rate is taken from ReqRec if presented.")]
        public void EnchantmentRateShouldBeTakenFromReqRec()
        {
            var character = CreateCharacter();

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            weapon.EnchantmentLevel = 19;

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl2.Type, PerfectWeaponLapisia_Lvl2.TypeId);
            Assert.Equal(lapisia.EnchantRate * 100, character.LinkingManager.GetEnchantmentRate(weapon, lapisia, null));
        }

        [Fact]
        [Description("Enchantment gold is calculated right.")]
        public void EnchantmentGoldShouldBeCalculatedRight()
        {
            var character = CreateCharacter();

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);

            Assert.Equal((uint)1040000, character.LinkingManager.GetEnchantmentGold(armor));
            Assert.Equal((uint)2500000, character.LinkingManager.GetEnchantmentGold(weapon));
        }

        [Fact]
        [Description("Enchantment is not ok if item is not in inventory.")]
        public void EnchantmentNoItemInInventory()
        {
            var character = CreateCharacter();

            var result = character.LinkingManager.TryEnchant(0, 0, 0, 0);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment is not ok if lapisia does not have Special == to Lapisia.")]
        public void EnchantmentLapisiaTypeCheck()
        {
            var character = CreateCharacter();

            var dummyItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            character.InventoryManager.AddItem(dummyItem, "");

            var result = character.LinkingManager.TryEnchant(0, 0, dummyItem.Bag, dummyItem.Slot);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment is not ok if item is not presented in inventory or if item enchantment level is 20.")]
        public void EnchantmentLapisiaInInventoryCheck()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, AssaultLapisia.Type, AssaultLapisia.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var result = character.LinkingManager.TryEnchant(0, 0, lapisia.Bag, lapisia.Slot);
            Assert.False(result.Success);

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            armor.EnchantmentLevel = 20;
            character.InventoryManager.AddItem(armor, "");

            result = character.LinkingManager.TryEnchant(armor.Bag, armor.Slot, lapisia.Bag, lapisia.Slot);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment is not ok if not enough gold.")]
        public void EnchantmentCheckGold()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, AssaultLapisia.Type, AssaultLapisia.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            character.InventoryManager.AddItem(weapon, "");

            var result = character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia.Bag, lapisia.Slot);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment is not ok if wrong lapisia enchant level.")]
        public void EnchantmentCheckLapisiaLevel()
        {
            var character = CreateCharacter();

            var lapisia1 = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl1.Type, PerfectWeaponLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia1, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            weapon.EnchantmentLevel = 2;
            character.InventoryManager.AddItem(weapon, "");

            var result = character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia1.Bag, lapisia1.Slot);
            Assert.False(result.Success);

            var lapisia2 = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl2.Type, PerfectWeaponLapisia_Lvl2.TypeId);
            character.InventoryManager.AddItem(lapisia2, "");

            weapon.EnchantmentLevel = 0;

            result = character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia2.Bag, lapisia2.Slot);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment be raised by 1 level.")]
        public void EnchantmentLevelShoudRaiseByOne()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl1.Type, PerfectWeaponLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            character.InventoryManager.AddItem(weapon, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(weapon);

            var result = character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia.Bag, lapisia.Slot);
            Assert.True(result.Success);
        }

        [Fact]
        [Description("Enchantment increases min and max attack.")]
        public void EnchantmentIncreaseMinAndMaxAttack()
        {
            var character = CreateCharacter();
            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl1.Type, PerfectWeaponLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            character.InventoryManager.AddItem(weapon, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(weapon);

            character.InventoryManager.MoveItem(1, 1, 0, 5);
            Assert.NotNull(character.InventoryManager.Weapon);
            Assert.Equal(character.InventoryManager.Weapon.MinAttack, character.StatsManager.MinAttack);
            Assert.Equal(character.InventoryManager.Weapon.MaxAttack, character.StatsManager.MaxAttack);

            var oldMinAttack = character.InventoryManager.Weapon.MinAttack;
            var oldMaxAttack = character.InventoryManager.Weapon.MaxAttack;

            character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia.Bag, lapisia.Slot);

            Assert.Equal(weapon.MinAttack, character.StatsManager.MinAttack);
            Assert.Equal(weapon.MaxAttack, character.StatsManager.MaxAttack);
            Assert.True(weapon.MinAttack > oldMinAttack);
            Assert.True(weapon.MaxAttack > oldMaxAttack);
        }

        [Fact]
        [Description("Enchantment increases absorption.")]
        public void EnchantmentIncreaseAbsorption()
        {
            var character = CreateCharacter();
            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectArmorLapisia_Lvl1.Type, PerfectArmorLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            character.InventoryManager.AddItem(armor, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(armor);

            character.InventoryManager.MoveItem(1, 1, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(0, character.StatsManager.Absorption);

            character.LinkingManager.TryEnchant(armor.Bag, armor.Slot, lapisia.Bag, lapisia.Slot);

            Assert.Equal(5, character.StatsManager.Absorption);
        }

        [Fact]
        [Description("Enchantment, when failed can low level.")]
        public void EnchantmentCanLowLevelIfFails()
        {
            var character = CreateCharacter();
            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, ProtectorsLapisia.Type, ProtectorsLapisia.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            armor.EnchantmentLevel = 19;
            character.InventoryManager.AddItem(armor, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(armor);

            character.InventoryManager.MoveItem(1, 1, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(95, character.StatsManager.Absorption);

            character.LinkingManager.TryEnchant(armor.Bag, armor.Slot, lapisia.Bag, lapisia.Slot);
            Assert.Equal(90, character.StatsManager.Absorption);
            Assert.Equal(18, armor.EnchantmentLevel);
        }

        [Fact]
        [Description("Enchantment, when failed can break item.")]
        public void EnchantmentCanBreakItemIfFails()
        {
            var character = CreateCharacter();
            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, LapisiaBreakItem.Type, LapisiaBreakItem.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            armor.EnchantmentLevel = 19;
            character.InventoryManager.AddItem(armor, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(armor);

            character.InventoryManager.MoveItem(1, 1, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);

            character.LinkingManager.TryEnchant(armor.Bag, armor.Slot, lapisia.Bag, lapisia.Slot);
            Assert.Null(character.InventoryManager.Armor);
        }

        [Fact]
        [Description("Enchantment fails if item is weapon and lapisia is for armor.")]
        public void EnchantmentCheckItemAndLapisiaType()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectArmorLapisia_Lvl1.Type, PerfectArmorLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            character.InventoryManager.AddItem(weapon, "");

            var result = character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia.Bag, lapisia.Slot);
            Assert.False(result.Success);
        }

        [Fact]
        [Description("Enchantment of weapon should not add absorption.")]
        public void EnchantmentWeaponDoNotInfluenceAbsorption()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl1.Type, PerfectWeaponLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var weapon = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId);
            character.InventoryManager.AddItem(weapon, "");

            character.InventoryManager.Gold = character.LinkingManager.GetEnchantmentGold(weapon);
            
            character.LinkingManager.TryEnchant(weapon.Bag, weapon.Slot, lapisia.Bag, lapisia.Slot);

            Assert.Equal(1, weapon.EnchantmentLevel);

            character.InventoryManager.MoveItem(weapon.Bag, weapon.Slot, 0, 5);
            Assert.Equal(0, character.Absorption);
        }

        [Fact]
        [Description("Enchantment fails if item is armor and lapisia is for weapon.")]
        public void EnchantmentCheckItemAndLapisiaType2()
        {
            var character = CreateCharacter();

            var lapisia = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectWeaponLapisia_Lvl1.Type, PerfectWeaponLapisia_Lvl1.TypeId);
            character.InventoryManager.AddItem(lapisia, "");

            var armor = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId);
            character.InventoryManager.AddItem(armor, "");

            var result = character.LinkingManager.TryEnchant(armor.Bag, armor.Slot, lapisia.Bag, lapisia.Slot);
            Assert.False(result.Success);
        }
    }
}
