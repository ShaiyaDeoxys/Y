using Imgeneus.Game.Skills;
using Imgeneus.World.Game.Attack;
using Xunit;

namespace Imgeneus.World.Tests.MobTests
{
    public class MobAITest : BaseTest
    {
        [Fact]
        public void MobCanFindPlayerOnMap()
        {
            var map = testMap;
            var mob = CreateMob(Wolf.Id, map);

            var character = CreateCharacter(map);

            Assert.True(mob.AIManager.TryGetEnemy());
            Assert.Equal(mob.AttackManager.Target, character);
        }

        [Fact]
        public void MobCanKillPlayer()
        {
            var map = testMap;
            var mob = CreateMob(CrypticImmortal.Id, map);

            var character = CreateCharacter(map);

            character.HealthManager.IncreaseHP(1);
            Assert.True(character.HealthManager.CurrentHP > 0);

            mob.AIManager.TryGetEnemy();

            Assert.True(character.HealthManager.IsDead);
        }

        [Fact]
        public void MobWontSeePlayerInStealth()
        {
            var map = testMap;
            var mob = CreateMob(CrypticImmortal.Id, map);

            var character = CreateCharacter();
            character.SkillsManager.UseSkill(new Skill(Stealth, 0, 0), character);

            Assert.False(mob.AIManager.TryGetEnemy());
        }

        [Fact]
        public void MobWontSeePlayerInMonsterShape()
        {
            var map = testMap;
            var mob = CreateMob(CrypticImmortal.Id, map);

            var character = CreateCharacter(map);
            character.SkillsManager.UseSkill(new Skill(Evolution, 0, 0), character, character);

            Assert.False(mob.AIManager.TryGetEnemy());
        }

        [Fact]
        public void MobCanNotMoveIfImmobilized()
        {
            var map = testMap;
            var mob = CreateMob(Wolf.Id, map);
            mob.MovementManager.PosX = 10;
            mob.MovementManager.PosY = 10;
            mob.MovementManager.PosZ = 10;

            mob.AIManager.Move(20, 20);
            Assert.NotEqual(10, mob.MovementManager.PosX);
            Assert.NotEqual(10, mob.MovementManager.PosZ);

            var newX = mob.MovementManager.PosX;
            var newZ = mob.MovementManager.PosZ;
            mob.BuffsManager.AddBuff(new Skill(MagicRoots_Lvl1, 0, 0), null);

            mob.AIManager.Move(20, 20);
            Assert.Equal(newX, mob.MovementManager.PosX);
            Assert.Equal(newZ, mob.MovementManager.PosZ);
        }

        [Fact]
        public void MobSelectsTargetBasedOnDamageMadeAndRec()
        {
            var map = testMap;
            var mob = CreateMob(Wolf.Id, map);
            Assert.Equal(mob.HealthManager.CurrentHP, mob.HealthManager.MaxHP);

            var character1 = CreateCharacter(map);
            character1.HealthManager.FullRecover();
            character1.StatsManager.WeaponMinAttack = 1;
            character1.StatsManager.WeaponMaxAttack = 1;

            var character2 = CreateCharacter(map);
            character2.HealthManager.FullRecover();
            character2.StatsManager.WeaponMinAttack = 5;
            character2.StatsManager.WeaponMaxAttack = 5;

            character2.SkillsManager.UseSkill(new Skill(BloodyArc, 0, 0), character2);
            character1.SkillsManager.UseSkill(new Skill(BloodyArc, 0, 0), character1);

            Assert.Equal(character2, mob.AttackManager.Target); // Character 2 because made more damage.

            character1.StatsManager.ExtraRec = 100;

            character2.SkillsManager.UseSkill(new Skill(BloodyArc, 0, 0), character2);
            character1.SkillsManager.UseSkill(new Skill(BloodyArc, 0, 0), character1);

            Assert.Equal(character1, mob.AttackManager.Target); // Character 1 because has more rec.
        }
    }
}
