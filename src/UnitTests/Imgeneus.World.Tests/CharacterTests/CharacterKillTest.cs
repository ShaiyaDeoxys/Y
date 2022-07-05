using Imgeneus.Database.Entities;
using Imgeneus.World.Game;
using Imgeneus.World.Game.PartyAndRaid;
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
            characterToKill.HealthManager.OnDead += (uint senderId, IKiller killer) =>
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

        [Fact]
        [Description("Character should get kills, when killed opposite country.")]
        public void CharacterGetKills()
        {
            var map = testMap;
            var characterToKill = CreateCharacter(map);
            characterToKill.HealthManager.IncreaseHP(characterToKill.HealthManager.MaxHP);

            var killer = CreateCharacter(map, country: Fraction.Dark);
            Assert.Equal((uint)0, killer.KillsManager.Kills);

            characterToKill.HealthManager.DecreaseHP(characterToKill.HealthManager.MaxHP, killer);

            Assert.Equal((uint)1, killer.KillsManager.Kills);
        }

        [Fact]
        [Description("Characters in party should get kills, when killed opposite country, if they not far away.")]
        public void CharacterPartyGetKills()
        {
            var map = testMap;
            var characterToKill = CreateCharacter(map);
            characterToKill.HealthManager.IncreaseHP(characterToKill.HealthManager.MaxHP);

            var killer1 = CreateCharacter(map, country: Fraction.Dark);
            var killer2 = CreateCharacter(map, country: Fraction.Dark);
            var killer3 = CreateCharacter(testMap, country: Fraction.Dark); // On another map.
            
            Assert.Equal((uint)0, killer1.KillsManager.Kills);
            Assert.Equal((uint)0, killer2.KillsManager.Kills);
            Assert.Equal((uint)0, killer3.KillsManager.Kills);

            var party = new Party(packetFactoryMock.Object);
            killer1.PartyManager.Party = party;
            killer2.PartyManager.Party = party;
            killer3.PartyManager.Party = party;

            characterToKill.HealthManager.DecreaseHP(characterToKill.HealthManager.MaxHP, killer1);

            Assert.Equal((uint)1, killer1.KillsManager.Kills);
            Assert.Equal((uint)1, killer2.KillsManager.Kills);
            Assert.Equal((uint)0, killer3.KillsManager.Kills);
        }
    }
}
