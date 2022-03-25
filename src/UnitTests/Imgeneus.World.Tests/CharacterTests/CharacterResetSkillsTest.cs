using Imgeneus.Database.Entities;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterResetSkillsTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to reset skills.")]
        public void ResetSkillsTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;
            character.LevelingManager.TryChangeLevel(2);
            Assert.Equal(7, character.SkillsManager.SkillPoints);
            Assert.Empty(character.SkillsManager.Skills);

            character.SkillsManager.TryLearnNewSkill(1, 1);
            Assert.Equal(6, character.SkillsManager.SkillPoints);
            Assert.NotEmpty(character.SkillsManager.Skills);

            character.SkillsManager.ResetSkills();
            Assert.Equal(7, character.SkillsManager.SkillPoints);
            Assert.Empty(character.SkillsManager.Skills);
        }
    }
}
