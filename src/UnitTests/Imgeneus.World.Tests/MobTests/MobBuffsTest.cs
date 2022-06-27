using Imgeneus.Game.Skills;
using Imgeneus.World.Game.Buffs;
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
            mob.BuffsManager.OnBuffAdded += (uint senderId, Buff newBuff) =>
            {
                buff = newBuff;
            };

            mob.BuffsManager.AddBuff(new Skill(MagicRoots_Lvl1, 0, 0), null);
            Assert.Single(mob.BuffsManager.ActiveBuffs);
            Assert.NotNull(buff);
        }

        [Fact]
        [Description("Mob can have resistanse to Misfortune.")]
        public void MobCanHaveDebuffResistance()
        {
            var map = testMap;
            var mob = CreateMob(Wolf.Id, map);
            var character = CreateCharacter(map);

            character.SkillsManager.UseSkill(new Skill(Misfortune, 0, 0), character, mob);

            Assert.Empty(mob.BuffsManager.ActiveBuffs); // Wolf has resistanse to Misfortune
        }
    }
}
