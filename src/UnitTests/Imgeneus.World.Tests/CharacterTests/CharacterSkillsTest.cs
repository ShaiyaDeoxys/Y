using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Skills;
using Parsec.Shaiya.Skill;
using System.ComponentModel;
using Xunit;
using Element = Imgeneus.Database.Constants.Element;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterSkillsTest : BaseTest
    {
        [Fact]
        [Description("Dispel should clear debuffs.")]
        public void DispelTest()
        {
            var character = CreateCharacter();

            character.BuffsManager.AddBuff(new Skill(Panic_Lvl1, 0, 0), null);
            Assert.Single(character.BuffsManager.ActiveBuffs);

            character.SkillsManager.UsedDispelSkill(new Skill(Dispel, 0, 0), character);
            Assert.Empty(character.BuffsManager.ActiveBuffs);
        }

        [Fact]
        [Description("With untouchable all attacks should miss.")]
        public void UntouchableTest()
        {
            var character = CreateCharacter();

            var character2 = CreateCharacter();
            character2.AttackManager.AlwaysHit = false;

            var attackSuccess = (character2 as IKiller).AttackManager.AttackSuccessRate(character, TypeAttack.ShootingAttack, new Skill(BullsEye, 0, 0));
            Assert.True(attackSuccess); // Bull eye has 100% success rate.

            // Use untouchable.
            character.BuffsManager.AddBuff(new Skill(Untouchable, 0, 0), null);
            Assert.Single(character.BuffsManager.ActiveBuffs);

            attackSuccess = (character2 as IKiller).AttackManager.AttackSuccessRate(character, TypeAttack.ShootingAttack, new Skill(BullsEye, 0, 0));
            Assert.False(attackSuccess); // When target is untouchable, bull eye is going to fail.
        }

        [Fact]
        [Description("Archer should miss if fighter used 'FleetFoot' skill.")]
        public void FleetFootTest()
        {
            var fighter = CreateCharacter();
            var archer = CreateCharacter(profession: CharacterProfession.Archer);
            archer.AttackManager.AlwaysHit = false;

            fighter.BuffsManager.AddBuff(new Skill(FleetFoot, 0, 0), null);
            Assert.Single(fighter.BuffsManager.ActiveBuffs);

            var attackSuccess = (archer as IKiller).AttackManager.AttackSuccessRate(fighter, TypeAttack.ShootingAttack);
            Assert.False(attackSuccess);
        }

        [Fact]
        [Description("Transformation should raise shape change event.")]
        public void TransformationTest()
        {
            var character = CreateCharacter();
            var shapeChangeCalled = false;
            character.ShapeManager.OnTranformated += (int sender, bool transformed) => shapeChangeCalled = transformed;

            character.BuffsManager.AddBuff(new Skill(Transformation, 0, 0), null);
            Assert.Single(character.BuffsManager.ActiveBuffs);

            Assert.True(shapeChangeCalled);
        }

        [Fact]
        [Description("BerserkersRage can be activated and disactivated.")]
        public void BerserkersRageTest()
        {
            var character = CreateCharacter();
            var skill = new Skill(BerserkersRage, 0, 0);
            Assert.True(skill.CanBeActivated);

            character.BuffsManager.AddBuff(skill, null);

            Assert.Single(character.BuffsManager.ActiveBuffs);
            Assert.True(skill.IsActivated);

            character.BuffsManager.AddBuff(skill, null);

            Assert.Empty(character.BuffsManager.ActiveBuffs);
            Assert.False(skill.IsActivated);
        }

        [Fact]
        [Description("Wild Rage should decrease HP and SP.")]
        public void WildRageTest()
        {
            var character1 = CreateCharacter();
            var character2 = CreateCharacter();

            var result = character1.AttackManager.CalculateDamage(character2, TypeAttack.PhysicalAttack, Element.None, 1, 5, 0, 0, new Skill(WildRage, 0, 0));
            Assert.True(result.Damage.HP > WildRage.DamageHP);
            Assert.Equal(result.Damage.SP, WildRage.DamageSP);
        }

        [Fact]
        [Description("Deadly Strike should generate 2 range attacks.")]
        public void DeadlyStrikeTest()
        {
            var character1 = CreateCharacter();
            var character2 = CreateCharacter();

            var numberOfRangeAttacks = 0;
            character1.SkillsManager.OnUsedRangeSkill += (int senderId, IKillable killable, Skill skill, AttackResult res) => numberOfRangeAttacks++;

            character1.SkillsManager.UseSkill(new Skill(DeadlyStrike, 0, 0), character1, character2);

            Assert.Equal(2, numberOfRangeAttacks);
        }

        [Fact]
        [Description("Nettle Sting should generate as many range attacks as many targets it got.")]
        public void NettleStingTest()
        {
            var map = testMap;
            var character1 = CreateCharacter(map: map);
            var character2 = CreateCharacter(map: map, country: Fraction.Dark);
            var character3 = CreateCharacter(map: map, country: Fraction.Dark);
            var character4 = CreateCharacter(map: map, country: Fraction.Dark);

            var numberOfRangeAttacks = 0;
            character1.SkillsManager.OnUsedRangeSkill += (int senderId, IKillable killable, Skill skill, AttackResult res) => numberOfRangeAttacks++;

            character1.StatsManager.WeaponMinAttack = 1;
            character1.StatsManager.WeaponMaxAttack = 1;

            var character2GotDamage = false;
            var character3GotDamage = false;
            var character4GotDamage = false;

            character2.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character2GotDamage = true;
            character3.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character3GotDamage = true;
            character4.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character4GotDamage = true;

            character1.SkillsManager.UseSkill(new Skill(NettleSting, 0, 0), character1, character2);

            Assert.Equal(3, numberOfRangeAttacks);
            Assert.True(character2GotDamage);
            Assert.True(character3GotDamage);
            Assert.True(character4GotDamage);
        }

        [Fact]
        [Description("Nettle Sting can be used only with Spear.")]
        public void NettleStingNeedsSpearTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter(country: Fraction.Dark);
            character.InventoryManager.AddItem(new Item(databasePreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId));
            character.InventoryManager.MoveItem(1, 0, 0, 5);

            character.SkillsManager.CanUseSkill(new Skill(NettleSting, 0, 0), character2, out var result);
            Assert.Equal(AttackSuccess.WrongEquipment, result);

            // Give spear.
            character.InventoryManager.AddItem(new Item(databasePreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Spear.Type, Spear.TypeId));
            character.InventoryManager.MoveItem(1, 0, 0, 5);

            character.SkillsManager.CanUseSkill(new Skill(NettleSting, 0, 0), character2, out result);
            Assert.Equal(AttackSuccess.Normal, result);
        }

        [Fact]
        [Description("Eraser should make x2 hp damage and should kill skill owner.")]
        public void EraserTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter(country: Fraction.Dark);

            character.HealthManager.FullRecover();
            Assert.Equal(character.HealthManager.MaxHP, character.HealthManager.CurrentHP);

            var result = character.AttackManager.CalculateAttackResult(new Skill(Eraser, 0, 0), character2, Element.None, 0, 0, 0, 0);
            Assert.Equal(character.HealthManager.CurrentHP * 2, result.Damage.HP);

            character.SkillsManager.UseSkill(new Skill(Eraser, 0, 0), character, character2);
            Assert.True(character.HealthManager.IsDead);
        }

        [Fact]
        [Description("BloodyArc should hit enemy near caster without providing target.")]
        public void BloodyArcTest()
        {
            var map = testMap;
            var character1 = CreateCharacter(map: map);
            character1.StatsManager.WeaponMinAttack = 1;
            character1.StatsManager.WeaponMaxAttack = 1;
            var character2 = CreateCharacter(map: map, country: Fraction.Dark);

            character2.HealthManager.FullRecover();
            Assert.Equal(character2.HealthManager.MaxHP, character2.HealthManager.CurrentHP);

            character1.SkillsManager.UseSkill(new Skill(BloodyArc, 0, 0), character1);

            Assert.True(character2.HealthManager.CurrentHP < character2.HealthManager.MaxHP);
        }
    }
}
