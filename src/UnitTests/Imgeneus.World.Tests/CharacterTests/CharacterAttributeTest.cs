using Imgeneus.Database.Entities;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterAttributeTest : BaseTest
    {
        [Fact]
        [Description("Stats Points should be updated when setting a new value.")]
        public void SetStatPointTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;
            character.StatsManager.TrySetStats(statPoints: 10);

            Assert.NotEqual(105, character.StatsManager.StatPoint);
            character.StatsManager.TrySetStats(statPoints: 105);
            Assert.Equal(105, character.StatsManager.StatPoint);

            character.StatsManager.TrySetStats(statPoints: ushort.MinValue);
            Assert.Equal(ushort.MinValue, character.StatsManager.StatPoint);

            character.StatsManager.TrySetStats(statPoints: ushort.MaxValue);
            Assert.Equal(ushort.MaxValue, character.StatsManager.StatPoint);
        }

        [Fact]
        [Description("Skill Points should be updated when setting a new value.")]
        public void SetSkillPointTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;
            character.StatsManager.TrySetStats(statPoints: 10);

            Assert.NotEqual(105, character.SkillsManager.SkillPoints);
            character.SkillsManager.TrySetSkillPoints(105);
            Assert.Equal(105, character.SkillsManager.SkillPoints);

            character.SkillsManager.TrySetSkillPoints(ushort.MinValue);
            Assert.Equal(ushort.MinValue, character.SkillsManager.SkillPoints);

            character.SkillsManager.TrySetSkillPoints(ushort.MaxValue);
            Assert.Equal(ushort.MaxValue, character.SkillsManager.SkillPoints);
        }

        [Fact]
        [Description("Character Kills should be updated when setting a new value.")]
        public void SetKills()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            character.KillsManager.Kills = 10;
            Assert.NotEqual((uint)105, character.KillsManager.Kills);
            character.KillsManager.Kills = 105;
            Assert.Equal((uint)105, character.KillsManager.Kills);

            character.KillsManager.Kills = ushort.MinValue;
            Assert.Equal(ushort.MinValue, character.KillsManager.Kills);

            character.KillsManager.Kills = ushort.MaxValue;
            Assert.Equal(ushort.MaxValue, character.KillsManager.Kills);
        }

        [Fact]
        [Description("Character Deaths should be updated when setting a new value.")]
        public void SetDeaths()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            character.KillsManager.Deaths = 10;
            Assert.NotEqual((uint)105, character.KillsManager.Deaths);
            character.KillsManager.Deaths = 105;
            Assert.Equal((uint)105, character.KillsManager.Deaths);

            character.KillsManager.Deaths = ushort.MinValue;
            Assert.Equal(ushort.MinValue, character.KillsManager.Deaths);

            character.KillsManager.Deaths = ushort.MaxValue;
            Assert.Equal(ushort.MaxValue, character.KillsManager.Deaths);
        }
    }
}