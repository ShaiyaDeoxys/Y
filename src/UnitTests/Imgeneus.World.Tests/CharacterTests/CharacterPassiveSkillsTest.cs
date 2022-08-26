using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Speed;
using System.Collections.Generic;
using Xunit;

namespace Imgeneus.World.Tests
{
    public class CharacterPassiveSkillsTest : BaseTest
    {
        public CharacterPassiveSkillsTest()
        {
            definitionsPreloader
                .SetupGet((preloader) => preloader.Items)
                .Returns(new Dictionary<(byte Type, byte TypeId), DbItem>() {
                    { (1,1), new DbItem() { Type = 1, TypeId = 1, ItemName = "Long Sword", AttackTime = 5, MinAttack = 10, PlusAttack = 5 } }
                });
        }

        [Fact]
        public void StrengthTrainingTest()
        {
            var character = CreateCharacter();

            Assert.Equal(0, character.StatsManager.MinAttack);
            Assert.Equal(0, character.StatsManager.MaxAttack);

            character.BuffsManager.AddBuff(new Skill(StrengthTraining, 0, 0), null);

            Assert.Equal(StrengthTraining.AbilityValue1, character.StatsManager.MinAttack);
            Assert.Equal(StrengthTraining.AbilityValue1, character.StatsManager.MaxAttack);
        }

        [Fact]
        public void ManaTrainingTest()
        {
            var character = CreateCharacter();

            Assert.Equal(200, character.HealthManager.MaxMP);

            character.BuffsManager.AddBuff(new Skill(ManaTraining, 0, 0), null);

            Assert.Equal(200 + ManaTraining.AbilityValue1, character.HealthManager.MaxMP);
        }

        [Fact]
        public void WeaponMasteryTest()
        {
            var character = CreateCharacter();
            var sword = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, 1, 1);
            Assert.Equal(AttackSpeed.None, character.SpeedManager.TotalAttackSpeed);

            character.InventoryManager.Weapon = sword;
            Assert.Equal(AttackSpeed.Normal, character.SpeedManager.TotalAttackSpeed);

            // Learn passive skill lvl 1.
            character.SkillsManager.TryLearnNewSkill(15, 1);
            Assert.Equal(AttackSpeed.ABitFast, character.SpeedManager.TotalAttackSpeed);

            // Learn passive skill lvl 2.
            character.SkillsManager.TryLearnNewSkill(15, 2);
            Assert.Equal(AttackSpeed.Fast, character.SpeedManager.TotalAttackSpeed);
        }

        [Fact]
        public void WeaponPowerUpTest()
        {
            var character = CreateCharacter();
            var sword = new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, 1, 1);
            character.InventoryManager.Weapon = sword;

            Assert.Equal(10, character.StatsManager.MinAttack);
            Assert.Equal(15, character.StatsManager.MaxAttack);

            character.SkillsManager.TryLearnNewSkill(MainWeaponPowerUp.SkillId, MainWeaponPowerUp.SkillLevel);
            Assert.Equal(10 + MainWeaponPowerUp.Weaponvalue, character.StatsManager.MinAttack);
            Assert.Equal(15 + MainWeaponPowerUp.Weaponvalue, character.StatsManager.MaxAttack);
        }
    }
}
