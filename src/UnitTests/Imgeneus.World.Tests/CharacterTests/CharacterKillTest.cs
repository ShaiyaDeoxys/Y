using Imgeneus.Database.Entities;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Player;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterKillTest : BaseTest
    {
        [Fact]
        [Description("Character killer should be the character, that did max damage")]
        public void Character_TestKillerCalculation()
        {
            var characterToKill = CreateCharacter();
            characterToKill.HealthManager.IncreaseHP(characterToKill.HealthManager.MaxHP);

            var killer1 = CreateCharacter();
            var killer2 = CreateCharacter();
            IKiller finalKiller = null;
            characterToKill.HealthManager.OnDead += (int senderId, IKiller killer) =>
            {
                finalKiller = killer;
            };

            var littleHP = characterToKill.HealthManager.CurrentHP / 5;
            var allHP = characterToKill.HealthManager.MaxHP;

            characterToKill.HealthManager.DecreaseHP(littleHP, killer1);
            Assert.Equal(characterToKill.HealthManager.MaxHP - littleHP, characterToKill.HealthManager.CurrentHP);
            characterToKill.HealthManager.DecreaseHP(characterToKill.HealthManager.MaxHP, killer2);
            Assert.Equal(0, characterToKill.HealthManager.CurrentHP);

            Assert.True(characterToKill.HealthManager.IsDead);
            Assert.Equal(killer2, finalKiller);
        }
    }
}
