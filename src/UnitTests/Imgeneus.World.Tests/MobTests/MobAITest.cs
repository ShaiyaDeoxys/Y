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

            var character = CreateCharacter();

            map.LoadPlayer(character);
            map.AddMob(mob);

            Assert.True(mob.AIManager.TryGetEnemy());
            Assert.Equal(mob.AttackManager.Target, character);
        }

        [Fact]
        public void MobCanKillPlayer()
        {
            var map = testMap;
            var mob = CreateMob(CrypticImmortal.Id, map);

            var character = CreateCharacter();

            character.HealthManager.IncreaseHP(1);
            Assert.True(character.HealthManager.CurrentHP > 0);

            map.LoadPlayer(character);
            map.AddMob(mob);

            Assert.True(mob.AIManager.TryGetEnemy());
            mob.AIManager.Attack(0, Database.Constants.Element.None, 100, 100);

            Assert.True(character.HealthManager.IsDead);
        }

        [Fact]
        public void MobWontSeePlayerInStealth()
        {
            var map = testMap;
            var mob = CreateMob(CrypticImmortal.Id, map);

            var character = CreateCharacter();
            character.SkillsManager.PerformSkill(new Skill(Stealth, 0, 0), character, character, character, new AttackResult());

            map.LoadPlayer(character);
            map.AddMob(mob);

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
    }
}
