using System.ComponentModel;
using System.Threading.Tasks;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Stats;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterStatsTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to reset stats.")]
        public void ResetStatTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;

            ushort level = 80;
            character.LevelProvider.Level = level;
            character.InventoryManager.TryResetStats();

            Assert.Equal(12 + 79, character.StatsManager.Strength); // 12 is default + 1 str per each level
            Assert.Equal(11, character.StatsManager.Dexterity);
            Assert.Equal(10, character.StatsManager.Reaction);
            Assert.Equal(8, character.StatsManager.Intelligence);
            Assert.Equal(9, character.StatsManager.Wisdom);
            Assert.Equal(10, character.StatsManager.Luck);
            Assert.Equal(711, character.StatsManager.StatPoint);
        }

        [Fact]
        [Description("Stats should be updated when setting a new value.")]
        public void SetStatTest()
        {
            var character = CreateCharacter();

            character.AdditionalInfoManager.Grow = Mode.Ultimate;
            character.StatsManager.TrySetStats(str: 1);
            character.StatsManager.TrySetStats(dex: 2);
            character.StatsManager.TrySetStats(rec: 3);
            character.StatsManager.TrySetStats(intl: 4);
            character.StatsManager.TrySetStats(wis: 5);
            character.StatsManager.TrySetStats(luc: 6);

            ushort newStatValue = 77;

            Assert.NotEqual(newStatValue, character.StatsManager.Strength);
            Assert.NotEqual(newStatValue, character.StatsManager.Dexterity);
            Assert.NotEqual(newStatValue, character.StatsManager.Intelligence);
            Assert.NotEqual(newStatValue, character.StatsManager.Reaction);
            Assert.NotEqual(newStatValue, character.StatsManager.Wisdom);
            Assert.NotEqual(newStatValue, character.StatsManager.Luck);

            character.StatsManager.TrySetStats(str: newStatValue);
            character.StatsManager.TrySetStats(dex: newStatValue);
            character.StatsManager.TrySetStats(intl: newStatValue);
            character.StatsManager.TrySetStats(rec: newStatValue);
            character.StatsManager.TrySetStats(wis: newStatValue);
            character.StatsManager.TrySetStats(luc: newStatValue);

            Assert.Equal(newStatValue, character.StatsManager.Strength);
            Assert.Equal(newStatValue, character.StatsManager.Dexterity);
            Assert.Equal(newStatValue, character.StatsManager.Intelligence);
            Assert.Equal(newStatValue, character.StatsManager.Reaction);
            Assert.Equal(newStatValue, character.StatsManager.Wisdom);
            Assert.Equal(newStatValue, character.StatsManager.Luck);
        }

        [Fact]
        [Description("Character's max HP, MP and SP should be incremented with REC, WIS and DEX")]
        public void VitalityTest()
        {
            var character = CreateCharacter();

            character.StatsManager.TrySetStats(rec: 0);
            character.StatsManager.TrySetStats(wis: 0);
            character.StatsManager.TrySetStats(dex: 0);

            var previousHP = character.HealthManager.MaxHP;
            var previousMP = character.HealthManager.MaxMP;
            var previousSP = character.HealthManager.MaxSP;

            character.StatsManager.TrySetStats(rec: 5);
            character.StatsManager.TrySetStats(wis: 10);
            character.StatsManager.TrySetStats(dex: 15);

            Assert.Equal(previousHP + 25, character.HealthManager.MaxHP);
            Assert.Equal(previousMP + 50, character.HealthManager.MaxMP);
            Assert.Equal(previousSP + 75, character.HealthManager.MaxSP);
        }

        [Fact]
        [Description("It should be possible to set victories and defeats.")]
        public void SetVictoriesAndDefeatsTest()
        {
            var character = CreateCharacter();
            Assert.Equal((uint)0, character.KillsManager.Victories);
            Assert.Equal((uint)0, character.KillsManager.Defeats);

            character.KillsManager.Victories = 10;
            character.KillsManager.Defeats = 20;

            Assert.Equal((uint)10, character.KillsManager.Victories);
            Assert.Equal((uint)20, character.KillsManager.Defeats);
        }
    }
}
