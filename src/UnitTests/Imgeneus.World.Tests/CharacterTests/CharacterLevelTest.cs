using Imgeneus.Database.Entities;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterLevelTest : BaseTest
    {
        [Fact]
        [Description("Character level should respect boundaries.")]
        public void LevelBoundariesTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            ushort maxLevel = 80;

            character.LevelingManager.TryChangeLevel(0);
            Assert.NotEqual(0, character.LevelProvider.Level);

            character.LevelingManager.TryChangeLevel((ushort)(maxLevel + 1));
            Assert.NotEqual(maxLevel + 1, character.LevelProvider.Level);

            character.LevelingManager.TryChangeLevel(1000);
            Assert.NotEqual(1000, character.LevelProvider.Level);
        }

        [Fact]
        [Description("Character level can't be changed to same level.")]
        public void LevelChangeTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            ushort maxLevel = 80;

            Assert.True(character.LevelingManager.TryChangeLevel(2));
            Assert.False(character.LevelingManager.TryChangeLevel(2));
            Assert.Equal(2, character.LevelProvider.Level);

            Assert.True(character.LevelingManager.TryChangeLevel(maxLevel));
            Assert.False(character.LevelingManager.TryChangeLevel(maxLevel));
            Assert.Equal(maxLevel, character.LevelProvider.Level);
        }

        [Fact]
        [Description("If changing back to level 1, exp should change to 0.")]
        public void Level1ExpShouldBeExp0Test()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            character.LevelingManager.TryChangeLevel(2);
            Assert.True(character.LevelingManager.Exp != 0);

            character.LevelingManager.TryChangeLevel(1);
            Assert.True(character.LevelingManager.Exp == 0);
        }
    }
}