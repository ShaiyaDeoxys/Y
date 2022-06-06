using Imgeneus.Game.Skills;
using Imgeneus.World.Game.Skills;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests
{
    public class CharacterBuffsTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to add a buff.")]
        public void Character_AddActiveBuff()
        {
            var character = CreateCharacter();
            Assert.Empty(character.BuffsManager.ActiveBuffs);

            var usedSkill = new Skill(skill1_level1, 1, 0);
            character.BuffsManager.AddBuff(usedSkill, character);
            Assert.NotEmpty(character.BuffsManager.ActiveBuffs);
        }

        [Fact]
        [Description("Buff with lower level should not override buff with the higher level.")]
        public void Character_BuffOflowerLevelCanNotOverrideHigherLevel()
        {
            var character = CreateCharacter();
            character.BuffsManager.AddBuff(new Skill(skill1_level2, 1, 0), character);

            Assert.Equal(skill1_level2.SkillId, character.BuffsManager.ActiveBuffs[0].Skill.SkillId);
            Assert.Equal(skill1_level2.SkillLevel, character.BuffsManager.ActiveBuffs[0].Skill.SkillLevel);

            character.BuffsManager.AddBuff(new Skill(skill1_level1, 1, 0), character);

            Assert.Equal(skill1_level2.SkillId, character.BuffsManager.ActiveBuffs[0].Skill.SkillId);
            Assert.Equal(skill1_level2.SkillLevel, character.BuffsManager.ActiveBuffs[0].Skill.SkillLevel);
        }

        [Fact]
        [Description("Buff of the same level is already applied, it should change reset time.")]
        public void Character_BuffOfSameLevelShouldChangeResetTime()
        {
            var character = CreateCharacter();
            var skill = new Skill(skill1_level2, 1, 0);

            character.BuffsManager.AddBuff(skill, character);
            var oldReselTime = character.BuffsManager.ActiveBuffs[0].ResetTime;

            character.BuffsManager.AddBuff(skill, character);
            Assert.True(character.BuffsManager.ActiveBuffs[0].ResetTime > oldReselTime && character.BuffsManager.ActiveBuffs[0].ResetTime != oldReselTime);
        }

        [Fact]
        [Description("Buff is cleared after player's death.")]
        public void Character_BuffClearedOnDeath()
        {
            var character = CreateCharacter();
            var leadership = new Skill(Leadership, 1, 0);
            var health_potion = new Skill(Skill_HealthRemedy_Level1, ISkillsManager.ITEM_SKILL_NUMBER, 0);

            character.BuffsManager.AddBuff(leadership, character);
            character.BuffsManager.AddBuff(health_potion, character);

            Assert.Equal(2, character.BuffsManager.ActiveBuffs.Count);
            Assert.Equal(Leadership.AbilityValue1, character.StatsManager.MinAttack);

            character.HealthManager.DecreaseHP(100, CreateCharacter());

            Assert.True(character.HealthManager.IsDead);
            Assert.Single(character.BuffsManager.ActiveBuffs);
            Assert.Equal(Skill_HealthRemedy_Level1.SkillId, character.BuffsManager.ActiveBuffs[0].Skill.SkillId);
            Assert.Equal(0, character.StatsManager.MinAttack);
        }
    }
}
