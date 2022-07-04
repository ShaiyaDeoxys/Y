using System.Linq;
using Xunit;

namespace Imgeneus.World.Tests
{
    public class MapMobRespawnTest : BaseTest
    {

        [Fact]
        public void MobCanRespawnAfterDeath()
        {
            var map = testMap;
            var mob = CreateMob(1, map);
            var character = CreateCharacter(map);

            Assert.NotNull(map.GetMob(mob.CellId, mob.Id));

            mob.HealthManager.DecreaseHP(mob.HealthManager.CurrentHP, character);
            Assert.Null(map.GetMob(mob.CellId, mob.Id));

            map.RebirthMob(mob);

            // Should rebirth with new id.
            var mobs = map.Cells[0].GetAllMobs(false);
            Assert.Single(mobs);
            Assert.Equal(Wolf.HP, mobs.ElementAt(0).HealthManager.CurrentHP);
            Assert.False(mobs.ElementAt(0).HealthManager.IsDead);
            Assert.True(mobs.ElementAt(0).Id != 0);
            Assert.True(mobs.ElementAt(0).Id != mob.Id);
        }
    }
}
