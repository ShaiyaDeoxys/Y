using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Game.Skills;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Inventory;
using Parsec.Shaiya.Skill;
using System.ComponentModel;
using System.Threading.Tasks;
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

            var numberOfUsedSkill = 0;
            character1.SkillsManager.OnUsedSkill += (int senderId, IKillable killable, Skill skill, AttackResult res) => numberOfUsedSkill++;

            character1.StatsManager.WeaponMinAttack = 1;
            character1.StatsManager.WeaponMaxAttack = 1;

            var character2GotDamage = false;
            var character3GotDamage = false;
            var character4GotDamage = false;

            character2.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character2GotDamage = true;
            character3.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character3GotDamage = true;
            character4.HealthManager.OnGotDamage += (int senderId, IKiller character1) => character4GotDamage = true;

            character1.SkillsManager.UseSkill(new Skill(NettleSting, 0, 0), character1, character2);

            Assert.Equal(2, numberOfRangeAttacks);
            Assert.Equal(1, numberOfUsedSkill);
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

        [Fact]
        [Description("IntervalTraining should decrease str and increase dex.")]
        public void IntervalTrainingTest()
        {
            var character = CreateCharacter();
            character.StatsManager.TrySetStats(str: 50);
            character.StatsManager.ExtraStr = 50;

            Assert.Equal(100, character.StatsManager.TotalStr);
            Assert.Equal(0, character.StatsManager.TotalDex);

            character.BuffsManager.AddBuff(new Skill(IntervalTraining, 0, 0), null);

            Assert.Empty(character.BuffsManager.ActiveBuffs); // IntervalTraining is passive buff.
            Assert.Equal(96, character.StatsManager.TotalStr);
            Assert.Equal(4, character.StatsManager.TotalDex);
        }

        [Fact]
        [Description("MagicVeil should block 3 magic attacks.")]
        public void MagicVeilTest()
        {
            var character = CreateCharacter();
            var character2 = CreateCharacter();
            character2.AttackManager.AlwaysHit = false;

            character.BuffsManager.AddBuff(new Skill(MagicVeil, 0, 0), null);
            Assert.Single(character.BuffsManager.ActiveBuffs);

            var missed = 0;
            character2.SkillsManager.OnUsedSkill += (int senderId, IKillable target, Skill skill, AttackResult result) =>
            {
                if (result.Success == AttackSuccess.Miss)
                    missed++;
            };

            character2.SkillsManager.UseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, character);
            character2.SkillsManager.UseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, character);
            character2.SkillsManager.UseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, character);

            Assert.Equal(3, missed);
            Assert.Empty(character.BuffsManager.ActiveBuffs);
        }

        [Fact]
        [Description("EtainsEmbrace should require 850 MP.")]
        public void CheckNeedMPTest()
        {
            var character = CreateCharacter();
            Assert.Equal(0, character.HealthManager.CurrentMP);

            var canUse = character.SkillsManager.CanUseSkill(new Skill(EtainsEmbrace, 0, 0), null, out var result);
            Assert.False(canUse);
            Assert.Equal(AttackSuccess.NotEnoughMPSP, result);

            character.HealthManager.ExtraMP += 1000;
            character.HealthManager.FullRecover();

            canUse = character.SkillsManager.CanUseSkill(new Skill(EtainsEmbrace, 0, 0), null, out result);
            Assert.True(canUse);
            Assert.Equal(AttackSuccess.Normal, result);
        }

        [Fact]
        [Description("EtainsEmbrace should block all debuffs.")]
        public void EtainsEmbraceTest()
        {
            var character = CreateCharacter();
            character.SkillsManager.UseSkill(new Skill(EtainsEmbrace, 0, 0), character);

            Assert.Single(character.BuffsManager.ActiveBuffs);

            character.BuffsManager.AddBuff(new Skill(Panic_Lvl1, 0, 0), null);

            Assert.Single(character.BuffsManager.ActiveBuffs); // Panic won't be added.
        }

        [Fact]
        [Description("MagicMirror should mirrow magic damage.")]
        public void MagicMirrorTest()
        {
            var character1 = CreateCharacter();
            var character2 = CreateCharacter();

            character1.HealthManager.FullRecover();
            character2.HealthManager.FullRecover();

            Assert.Equal(character1.HealthManager.MaxHP, character1.HealthManager.CurrentHP);
            Assert.Equal(character2.HealthManager.MaxHP, character2.HealthManager.CurrentHP);

            character1.SkillsManager.UseSkill(new Skill(MagicMirror, 0, 0), character1);
            character2.SkillsManager.UseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, character1);

            Assert.Equal(character1.HealthManager.MaxHP, character1.HealthManager.CurrentHP);
            Assert.True(character2.HealthManager.MaxHP > character2.HealthManager.CurrentHP);
        }

        [Fact]
        [Description("PersistBarrier should stop, when player is moving.")]
        public void PersistBarrierShouldStopWhenMovingTest()
        {
            var character1 = CreateCharacter();
            character1.HealthManager.FullRecover();
            character1.SkillsManager.UseSkill(new Skill(PersistBarrier, 0, 0), character1);

            Assert.NotEmpty(character1.BuffsManager.ActiveBuffs);

            character1.MovementManager.PosX += 1;
            character1.MovementManager.RaisePositionChanged();

            Assert.Empty(character1.BuffsManager.ActiveBuffs);
        }

        [Fact]
        [Description("PersistBarrier should stop, when player has not enought mana.")]
        public async void PersistBarrierShouldStopWhenMPTest()
        {
            var character1 = CreateCharacter();
            character1.HealthManager.IncreaseHP(100);
            character1.HealthManager.CurrentMP = 15;
            character1.HealthManager.CurrentSP = 100;

            character1.SkillsManager.UseSkill(new Skill(PersistBarrier, 0, 0), character1);

            Assert.NotEmpty(character1.BuffsManager.ActiveBuffs);

            await Task.Delay(1100); // Wait ~ 1 sec

            Assert.Equal(1, character1.HealthManager.CurrentMP); // 7% from 200 is 14. 15 - 14 == 1 

            await Task.Delay(1100); // Wait ~ 1 sec

            Assert.Empty(character1.BuffsManager.ActiveBuffs); // Not enough mana, should cancel buff.
        }

        [Fact]
        [Description("PersistBarrier should stop, when player used any other skill.")]
        public void PersistBarrierShouldStopWhenUseOtherSkillTest()
        {
            var character1 = CreateCharacter();
            character1.HealthManager.IncreaseHP(100);
            character1.HealthManager.CurrentMP = 15;
            character1.HealthManager.CurrentSP = 100;

            character1.SkillsManager.UseSkill(new Skill(PersistBarrier, 0, 0), character1);

            Assert.NotEmpty(character1.BuffsManager.ActiveBuffs);

            character1.SkillsManager.UseSkill(new Skill(Dispel, 0, 0), character1);

            Assert.Empty(character1.BuffsManager.ActiveBuffs);
        }

        [Fact]
        [Description("Healing should increase HP.")]
        public void HealingTest()
        {
            var character1 = CreateCharacter();
            var priest = CreateCharacter();
            priest.StatsManager.TrySetStats(wis: 1);

            Assert.Equal(0, character1.HealthManager.CurrentHP);

            var result = new AttackResult();

            priest.SkillsManager.OnUsedSkill += (int senderId, IKillable target, Skill skill, AttackResult res) => result = res;
            priest.SkillsManager.UseSkill(new Skill(Healing, 0, 0), priest, character1);

            Assert.Equal(54, character1.HealthManager.CurrentHP);
            Assert.Equal(AttackSuccess.Normal, result.Success);
            Assert.Equal(54, result.Damage.HP);
        }

        [Fact]
        [Description("Healing Can not be used on opposite faction.")]
        public void HealingCanNotUseOnOppositeFactionTest()
        {
            var character1 = CreateCharacter(country: Fraction.Dark);
            var priest = CreateCharacter();

            var canUse = priest.SkillsManager.CanUseSkill(new Skill(Healing, 0, 0), character1, out var result);
            Assert.False(canUse);
            Assert.Equal(AttackSuccess.WrongTarget, result);
        }

        [Fact]
        [Description("Healing Can not be used on duel opponent.")]
        public void HealingCanNotUseOnDuelOpponentTest()
        {
            var character1 = CreateCharacter();
            var priest = CreateCharacter();

            character1.DuelManager.OpponentId = priest.Id;
            priest.DuelManager.OpponentId = character1.Id;

            var canUse = priest.SkillsManager.CanUseSkill(new Skill(Healing, 0, 0), character1, out var result);
            Assert.False(canUse);
            Assert.Equal(AttackSuccess.WrongTarget, result);
        }

        [Fact]
        [Description("Healing can be used on self.")]
        public void HealingCanBeUsedSelfTest()
        {
            var priest = CreateCharacter();
            var canUse = priest.SkillsManager.CanUseSkill(new Skill(Healing, 0, 0), priest, out var result);
            Assert.True(canUse);
        }

        [Fact]
        [Description("Healing can be used on self during duel.")]
        public void HealingCanBeUsedSelfDuringDuelTest()
        {
            var priest = CreateCharacter();
            priest.DuelManager.OpponentId = 1;

            var canUse = priest.SkillsManager.CanUseSkill(new Skill(Healing, 0, 0), priest, out var result);
            Assert.True(canUse);
        }

        [Fact]
        [Description("Healing can not be used on dead player.")]
        public void HealingCanNotBeUsedOnDeadPlayerTest()
        {
            var priest = CreateCharacter();
            var character1 = CreateCharacter();

            character1.HealthManager.DecreaseHP(1, CreateCharacter());
            var canUse = priest.SkillsManager.CanUseSkill(new Skill(Healing, 0, 0), character1, out var result);
            Assert.False(canUse);
            Assert.Equal(AttackSuccess.WrongTarget, result);
        }
    }
}
