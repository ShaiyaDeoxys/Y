using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.GemLinkingTests
{
    public class GemLinkingTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to link gem.")]
        public void GemAdd_LinkWithPerfectHammer()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_1.Type, Gem_Str_Level_1.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];
            Assert.NotNull(armor);
            Assert.NotNull(hammer);
            Assert.NotNull(gem);

            Assert.Equal(JustiaArmor.ConstStr, armor.Str);
            Assert.Null(armor.Gem1);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, hammer.Bag, hammer.Slot);

            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_1.ConstStr, armor.Str);
            Assert.NotNull(armor.Gem1);
        }

        [Fact]
        [Description("When linking fails, it should not break item, if gem ReqVg is 0. Gem should disappear after linking.")]
        public void GemAdd_FailNotBreakItem()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_1.Type, Gem_Str_Level_1.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var gem = character.InventoryManager.InventoryItems[(1, 1)];
            Assert.NotNull(armor);
            Assert.NotNull(gem);
            Assert.Null(armor.Gem1);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, 0, 0);

            armor = character.InventoryManager.InventoryItems[(1, 0)];

            Assert.NotNull(armor);
            Assert.Null(armor.Gem1);
            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 1))); // no gem
        }

        [Fact]
        [Description("When linking fails, it should break item, if gem ReqVg is 1. Gem and item should disappear after linking.")]
        public void GemAdd_FailBreakItem()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var gem = character.InventoryManager.InventoryItems[(1, 1)];
            Assert.NotNull(armor);
            Assert.NotNull(gem);
            Assert.Null(armor.Gem1);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, 0, 0);

            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 0))); // no armor
            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 1))); // no gem
        }

        [Fact]
        [Description("When linking gem to the item, that is on character, this should influence extra stats.")]
        public void GemAdd_ItemIsOnCharacter()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(JustiaArmor.ConstStr, character.StatsManager.TotalStr);

            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, character.InventoryManager.Armor.Bag, character.InventoryManager.Armor.Slot, hammer.Bag, hammer.Slot);

            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_7.ConstStr, character.StatsManager.TotalStr);

            // Take off armor.
            character.InventoryManager.MoveItem(0, 1, 1, 0);
            Assert.Equal(0, character.StatsManager.TotalStr);

            // Take on armor.
            character.InventoryManager.MoveItem(1, 0, 0, 1);
            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_7.ConstStr, character.StatsManager.TotalStr);
        }

        [Fact]
        [Description("When linking gem to the item, that is on character, fails (i.e. item is destroyed) this should influence extra stats.")]
        public void GemAdd_ItemIsOnCharacterBreak()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(JustiaArmor.ConstStr, character.StatsManager.TotalStr);

            var gem = character.InventoryManager.InventoryItems[(1, 1)];

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, character.InventoryManager.Armor.Bag, character.InventoryManager.Armor.Slot, 0, 0);

            Assert.Equal(0, character.StatsManager.TotalStr);
            Assert.Null(character.InventoryManager.Armor);
            Assert.Empty(character.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("Linking the same gems is forbidden")]
        public void GemAdd_SameGemsAreForbidden()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer1 = character.InventoryManager.InventoryItems[(1, 1)];
            var hammer2 = character.InventoryManager.InventoryItems[(1, 2)];
            var gem1 = character.InventoryManager.InventoryItems[(1, 3)];
            var gem2 = character.InventoryManager.InventoryItems[(1, 4)];

            Assert.NotNull(armor);
            Assert.NotNull(hammer1);
            Assert.NotNull(hammer2);
            Assert.NotNull(gem1);
            Assert.NotNull(gem2);

            character.LinkingManager.AddGem(gem1.Bag, gem1.Slot, armor.Bag, armor.Slot, hammer1.Bag, hammer1.Slot);
            Assert.NotNull(armor.Gem1);

            character.LinkingManager.AddGem(gem2.Bag, gem2.Slot, armor.Bag, armor.Slot, hammer2.Bag, hammer2.Slot);
            Assert.Null(armor.Gem2);

            hammer2 = character.InventoryManager.InventoryItems[(1, 2)];
            gem2 = character.InventoryManager.InventoryItems[(1, 4)];

            Assert.NotNull(hammer2);
            Assert.NotNull(gem2);
        }

        [Fact]
        [Description("Linking different gems is ok")]
        public void GemAdd_DifferentGemsIsOK()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_1.Type, Gem_Str_Level_1.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer1 = character.InventoryManager.InventoryItems[(1, 1)];
            var hammer2 = character.InventoryManager.InventoryItems[(1, 2)];
            var gem1 = character.InventoryManager.InventoryItems[(1, 3)];
            var gem2 = character.InventoryManager.InventoryItems[(1, 4)];

            Assert.NotNull(armor);
            Assert.NotNull(hammer1);
            Assert.NotNull(hammer2);
            Assert.NotNull(gem1);
            Assert.NotNull(gem2);

            character.LinkingManager.AddGem(gem1.Bag, gem1.Slot, armor.Bag, armor.Slot, hammer1.Bag, hammer1.Slot);
            Assert.NotNull(armor.Gem1);

            character.LinkingManager.AddGem(gem2.Bag, gem2.Slot, armor.Bag, armor.Slot, hammer2.Bag, hammer2.Slot);
            Assert.NotNull(armor.Gem2);

            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_1.ConstStr + Gem_Str_Level_7.ConstStr, armor.Str);
            Assert.Single(character.InventoryManager.InventoryItems); // After linking only armor should left.
        }

        [Fact]
        [Description("Linking should be possible only, if item has free slots.")]
        public void GemAdd_ItemMustHaveFreeSlots()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, WaterArmor.Type, WaterArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_1.Type, Gem_Str_Level_1.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];
            Assert.NotNull(armor);
            Assert.NotNull(hammer);
            Assert.NotNull(gem);
            Assert.Equal(0, armor.FreeSlots);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, hammer.Bag, hammer.Slot);
            Assert.Null(armor.Gem1);

            hammer = character.InventoryManager.InventoryItems[(1, 1)];
            gem = character.InventoryManager.InventoryItems[(1, 2)];
            Assert.NotNull(hammer);
            Assert.NotNull(gem);
        }

        [Fact]
        [Description("Lucky charm can save item, if it's in inventory.")]
        public void GemAdd_LuckyCharmSavesItem()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_7.Type, Gem_Str_Level_7.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, LuckyCharm.Type, LuckyCharm.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var gem = character.InventoryManager.InventoryItems[(1, 1)];
            var luckyCharm = character.InventoryManager.InventoryItems[(1, 2)];

            Assert.NotNull(armor);
            Assert.NotNull(gem);
            Assert.NotNull(luckyCharm);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, 0, 0);
            Assert.Null(armor.Gem1);

            armor = character.InventoryManager.InventoryItems[(1, 0)];
            Assert.NotNull(armor);
            Assert.Single(character.InventoryManager.InventoryItems); // Lucky charm was used and it saved armor.
        }

        [Fact]
        [Description("Lucky charm is used only, when it's needed.")]
        public void GemAdd_LuckyCharmUsedOnlyWhenNeeded()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Str_Level_1.Type, Gem_Str_Level_1.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, LuckyCharm.Type, LuckyCharm.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var gem = character.InventoryManager.InventoryItems[(1, 1)];
            var luckyCharm = character.InventoryManager.InventoryItems[(1, 2)];

            Assert.NotNull(armor);
            Assert.NotNull(gem);
            Assert.NotNull(luckyCharm);

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, 0, 0);
            Assert.Null(armor.Gem1);

            armor = character.InventoryManager.InventoryItems[(1, 0)];
            Assert.NotNull(armor);
            luckyCharm = character.InventoryManager.InventoryItems[(1, 2)];
            Assert.NotNull(luckyCharm);

            Assert.Equal(2, character.InventoryManager.InventoryItems.Count); // Only gem was used, lucky charm is still in inventory.
        }

        [Fact]
        [Description("It should be possible to exctract gem.")]
        public void GemRemove_ExtractWithPerfectHammer()
        {
            var character = CreateCharacter();

            var armorItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId)
            {
                Gem1 = new Gem(definitionsPreloader.Object, Gem_Str_Level_1.TypeId, 0)
            };

            character.InventoryManager.AddItem(armorItem, "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectExtractingHammer.Type, PerfectExtractingHammer.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer = character.InventoryManager.InventoryItems[(1, 1)];

            character.LinkingManager.RemoveGem(armor.Bag, armor.Slot, true, 0, hammer.Bag, hammer.Slot);
            Assert.Null(armor.Gem1);
            var gem = character.InventoryManager.InventoryItems[(1, 1)];
            Assert.NotNull(gem);
            Assert.Equal(Gem_Str_Level_1.Type, gem.Type);
            Assert.Equal(Gem_Str_Level_1.TypeId, gem.TypeId);
            Assert.Equal(2, character.InventoryManager.InventoryItems.Count); // armor + extracted gem
        }

        [Fact]
        [Description("It should be possible to exctract several gems without hammer. Extracted gems are added to inventory.")]
        public void GemRemove_ExtractWithoutPerfectHammer()
        {
            var character = CreateCharacter();

            var armorItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId)
            {
                Gem1 = new Gem(definitionsPreloader.Object, Gem_Str_Level_2.TypeId, 0),
                Gem2 = new Gem(definitionsPreloader.Object, Gem_Str_Level_3.TypeId, 1),
            };

            character.InventoryManager.AddItem(armorItem, "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            Assert.NotNull(armor);
            Assert.NotNull(armor.Gem1);
            Assert.NotNull(armor.Gem2);

            character.LinkingManager.RemoveGem(armor.Bag, armor.Slot, false, 0, 0, 0);

            armor = character.InventoryManager.InventoryItems[(1, 0)];
            Assert.NotNull(armor);
            Assert.Null(armor.Gem1);
            Assert.Null(armor.Gem2);

            var gem1 = character.InventoryManager.InventoryItems[(1, 1)];
            Assert.NotNull(gem1);
            Assert.Equal(gem1.Type, Gem_Str_Level_2.Type);
            Assert.Equal(gem1.TypeId, Gem_Str_Level_2.TypeId);

            var gem2 = character.InventoryManager.InventoryItems[(1, 2)];
            Assert.NotNull(gem2);
            Assert.Equal(gem2.Type, Gem_Str_Level_3.Type);
            Assert.Equal(gem2.TypeId, Gem_Str_Level_3.TypeId);

            Assert.Equal(3, character.InventoryManager.InventoryItems.Count); // armor + 2 extracted gem
        }

        [Fact]
        [Description("Extracting gem without hammer may break item, if gem ReqVg is 1. ")]
        public void GemRemove_ExtractWithoutPerfectHammerBreaksItem()
        {
            var character = CreateCharacter();

            var armorItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId)
            {
                Gem1 = new Gem(definitionsPreloader.Object, Gem_Str_Level_7.TypeId, 0)
            };

            character.InventoryManager.AddItem(armorItem, "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            Assert.NotNull(armor);
            Assert.NotNull(armor.Gem1);

            character.LinkingManager.RemoveGem(armor.Bag, armor.Slot, false, 0, 0, 0);

            Assert.Empty(character.InventoryManager.InventoryItems); // armor was destroyed
        }

        [Fact]
        [Description("When extracting gem from the item, that is on character, this should influence extra stats.")]
        public void GemRemove_ItemIsOnCharacter()
        {
            var character = CreateCharacter();

            var armorItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId)
            {
                Gem1 = new Gem(definitionsPreloader.Object, Gem_Str_Level_1.TypeId, 0)
            };
            character.InventoryManager.AddItem(armorItem, "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);
            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_1.ConstStr, character.StatsManager.TotalStr);

            character.LinkingManager.RemoveGem(character.InventoryManager.Armor.Bag, character.InventoryManager.Armor.Slot, false, 0, 0, 0);

            Assert.Equal(JustiaArmor.ConstStr, character.StatsManager.TotalStr);
        }

        [Fact]
        [Description("When extracting gem from the item, that is on character, this should influence extra stats. If gem ReqVg = 1, item should be broken.")]
        public void GemRemove_ItemIsOnCharacterBreakItem()
        {
            var character = CreateCharacter();

            var armorItem = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId)
            {
                Gem1 = new Gem(definitionsPreloader.Object, Gem_Str_Level_7.TypeId, 0)
            };
            character.InventoryManager.AddItem(armorItem, "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);
            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(JustiaArmor.ConstStr + Gem_Str_Level_7.ConstStr, character.StatsManager.TotalStr);

            character.LinkingManager.RemoveGem(character.InventoryManager.Armor.Bag, character.InventoryManager.Armor.Slot, false, 0, 0, 0);
            Assert.Null(character.InventoryManager.Armor);
            Assert.Equal(0, character.StatsManager.TotalStr);
            Assert.Empty(character.InventoryManager.InventoryItems);
        }

        [Fact]
        [Description("When Absorption gem is linked it should increase absorb value.")]
        public void Gem_Absorption()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Absorption_Level_4.Type, Gem_Absorption_Level_4.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, hammer.Bag, hammer.Slot);

            character.InventoryManager.MoveItem(1, 0, 0, 1);
            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(Gem_Absorption_Level_4.Exp, character.Absorption);
            Assert.Equal(Gem_Absorption_Level_4.Exp, character.InventoryManager.Armor.Absorb);
            Assert.True(character.Absorption > 0);
        }


        [Fact]
        [Description("When Absorption gem is linked it should increase absorb value.")]
        public void Gem_Absorption_ItemIsOnCharacter()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Absorption_Level_4.Type, Gem_Absorption_Level_4.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 1);

            Assert.NotNull(character.InventoryManager.Armor);
            Assert.Equal(0, character.Absorption);

            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];

            character.LinkingManager.AddGem(gem.Bag, gem.Slot, character.InventoryManager.Armor.Bag, character.InventoryManager.Armor.Slot, hammer.Bag, hammer.Slot);

            Assert.Equal(Gem_Absorption_Level_4.Exp, character.Absorption);
            Assert.True(character.Absorption > 0);
        }

        [Fact]
        [Description("When several absorption gems are linked, total Absorption value should be sum of gems Absorption.")]
        public void Gem_AbsorptionAreStackable()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, JustiaArmor.Type, JustiaArmor.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Absorption_Level_4.Type, Gem_Absorption_Level_4.TypeId), "");

            var armor = character.InventoryManager.InventoryItems[(1, 0)];
            var hammer = character.InventoryManager.InventoryItems[(1, 1)];
            var gem = character.InventoryManager.InventoryItems[(1, 2)];

            // Add lvl 4 gem.
            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, hammer.Bag, hammer.Slot);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, PerfectLinkingHammer.Type, PerfectLinkingHammer.TypeId), "");
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Gem_Absorption_Level_5.Type, Gem_Absorption_Level_5.TypeId), "");

            // Add lvl 5 gem.
            character.LinkingManager.AddGem(gem.Bag, gem.Slot, armor.Bag, armor.Slot, hammer.Bag, hammer.Slot);

            Assert.Equal(Gem_Absorption_Level_4.Exp + Gem_Absorption_Level_5.Exp, armor.Absorb);

            // Wear armor.
            character.InventoryManager.MoveItem(1, 0, 0, 1);
            Assert.Equal(Gem_Absorption_Level_4.Exp + Gem_Absorption_Level_5.Exp, character.Absorption);
        }
    }
}
