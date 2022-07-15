using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Country;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.BlessTests
{
    public class BlessTest : BaseTest
    {
        [Fact]
        [Description("Amount of bless can change max HP, SP and MP.")]
        public void Bless_Max_HP_SP_MP()
        {
            var character = CreateCharacter();

            Assert.Equal(100, character.HealthManager.MaxHP);
            Assert.Equal(200, character.HealthManager.MaxMP);
            Assert.Equal(300, character.HealthManager.MaxSP);
            Assert.Equal(CountryType.Light, character.CountryProvider.Country);

            character.BlessManager.LightAmount = IBlessManager.MAX_HP_SP_MP;
            Assert.Equal(120, character.HealthManager.MaxHP);
            Assert.Equal(240, character.HealthManager.MaxMP);
            Assert.Equal(360, character.HealthManager.MaxSP);

            character.BlessManager.LightAmount = IBlessManager.MAX_HP_SP_MP - 100;
            Assert.Equal(100, character.HealthManager.MaxHP);
            Assert.Equal(200, character.HealthManager.MaxMP);
            Assert.Equal(300, character.HealthManager.MaxSP);
        }
    }
}
