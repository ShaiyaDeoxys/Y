using Imgeneus.Game.Skills;
using Imgeneus.World.Tests;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.Game.Tests.DuelTests
{
    public class DuelTest : BaseTest
    {
        [Fact]
        [Description("Magic roots can be used on duel opponent.")]
        public void MagicRootsCanBeUsedInDuel()
        {
            var map = testMap;

            var character1 = CreateCharacter(map);
            var character2 = CreateCharacter(map);

            Assert.False(character1.SkillsManager.CanUseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, out var _));

            character1.DuelManager.OpponentId = character2.Id;
            character2.DuelManager.OpponentId = character1.Id;

            character1.DuelManager.Start();
            character2.DuelManager.Start();

            Assert.True(character1.SkillsManager.CanUseSkill(new Skill(MagicRoots_Lvl1, 0, 0), character2, out var _));
        }
    }
}
