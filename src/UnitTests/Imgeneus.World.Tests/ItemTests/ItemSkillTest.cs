using Imgeneus.Database.Constants;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Player;
using Parsec.Shaiya.Skill;
using System.ComponentModel;
using Xunit;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Tests.ItemTests
{
    public class ItemSkillTest : BaseTest
    {
        [Fact]
        [Description("Health Remedy should increase max hp.")]
        public void HPItemTest()
        {
            var character = CreateCharacter();
            Assert.Empty(character.BuffsManager.ActiveBuffs);
            Assert.Equal(100, character.HealthManager.MaxHP);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 5);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Item_HealthRemedy_Level_1.Type, Item_HealthRemedy_Level_1.TypeId), "");
            character.InventoryManager.TryUseItem(1, 0);

            Assert.NotEmpty(character.BuffsManager.ActiveBuffs);
            Assert.Equal(Skill_HealthRemedy_Level1.AbilityValue1 + 100, character.HealthManager.MaxHP);
        }

        [Fact]
        [Description("Blast A Remedy 1 should absorb damage.")]
        public void AbsorptionPotionTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter();

            var damage = (character2 as IKiller).AttackManager.CalculateAttackResult(character, Element.None, 100, 100, 0, 0, null);
            Assert.Equal(150, damage.Damage.HP);
            Assert.Equal(0, damage.Absorb);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 5);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Item_AbsorbRemedy.Type, Item_AbsorbRemedy.TypeId), "");
            character.InventoryManager.TryUseItem(1, 0);

            Assert.Equal(20, character.Absorption);

            damage = (character2 as IKiller).AttackManager.CalculateAttackResult(character, Element.None, 100, 100, 0, 0, null);
            Assert.Equal(130, damage.Damage.HP);
            Assert.Equal(20, damage.Absorb);
        }

        [Fact]
        [Description("Speedy Remedy should increase speed.")]
        public void SpeedPotionTest()
        {
            var character = CreateCharacter();
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");
            character.InventoryManager.MoveItem(1, 0, 0, 5);

            Assert.Equal(MoveSpeed.Normal, character.SpeedManager.TotalMoveSpeed);
            Assert.Empty(character.BuffsManager.ActiveBuffs);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, SpeedyRemedy.Type, SpeedyRemedy.TypeId), "");
            character.InventoryManager.TryUseItem(1, 0);

            Assert.Equal(MoveSpeed.Fast, character.SpeedManager.TotalMoveSpeed);
            Assert.Single(character.BuffsManager.ActiveBuffs);
        }
    }
}
