using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Skills;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.MobTests
{
    public class MobBuffsTest : BaseTest
    {

        [Fact]
        [Description("Mob sends notification, when it gets some buff/debuff.")]
        public void MobNotifiesWhenItGetsBuff()
        {
            var mob = CreateMob(Wolf.Id, testMap);
            Buff buff = null;
            mob.BuffsManager.OnBuffAdded += (int senderId, Buff newBuff) =>
            {
                buff = newBuff;
            };

            mob.BuffsManager.AddBuff(new Skill(MagicRoots_Lvl1, 0, 0), null);
            Assert.Single(mob.BuffsManager.ActiveBuffs);
            Assert.NotNull(buff);
        }
    }
}
