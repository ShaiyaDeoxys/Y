using Imgeneus.Game.Monster;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.MapTests
{
    public class PartyMapTest : BaseTest
    {
        [Fact]
        [Description("The Party map should notify as soon as all party members left the map and the party was destroyed.")]
        public void PartyMapDestroyWhenAllPlayersLeft()
        {
            var usualMap = testMap;
            var character1 = CreateCharacter(usualMap);
            var character2 = CreateCharacter(usualMap);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var partyMap = new PartyMap(party,
                                        Map.TEST_MAP_ID,
                                        new MapDefinition() { CreateType = CreateType.Party },
                                        new Svmap() { MapSize = 100, CellSize = 100 },
                                        new List<BossConfiguration>(),
                                        mapLoggerMock.Object,
                                        packetFactoryMock.Object,
                                        definitionsPreloader.Object,
                                        mobFactoryMock.Object,
                                        npcFactoryMock.Object,
                                        obeliskFactoryMock.Object,
                                        timeMock.Object);
            var allLeftWasCalled = false;
            partyMap.OnAllMembersLeft += (sender) =>
            {
                allLeftWasCalled = true;
            };

            partyMap.LoadPlayer(character1);
            partyMap.LoadPlayer(character2);

            character1.PartyManager.Party = null;

            Assert.False(allLeftWasCalled); // Should be called only after all members left.

            partyMap.UnloadPlayer(character1.Id);
            partyMap.UnloadPlayer(character2.Id);

            Assert.True(allLeftWasCalled);
        }

        [Fact]
        [Description("The Party map should notify as soon as all party members left the party.")]
        public void PartyMapDestroyWhenPartyDestroyed()
        {
            var usualMap = testMap;
            var character1 = CreateCharacter(usualMap);
            var character2 = CreateCharacter(usualMap);

            var party = new Party(packetFactoryMock.Object);
            character1.PartyManager.Party = party;
            character2.PartyManager.Party = party;

            var partyMap = new PartyMap(party,
                                        Map.TEST_MAP_ID,
                                        new MapDefinition() { CreateType = CreateType.Party },
                                        new Svmap() { MapSize = 100, CellSize = 100 },
                                        new List<BossConfiguration>(),
                                        mapLoggerMock.Object,
                                        packetFactoryMock.Object,
                                        definitionsPreloader.Object,
                                        mobFactoryMock.Object,
                                        npcFactoryMock.Object,
                                        obeliskFactoryMock.Object,
                                        timeMock.Object);
            var allLeftWasCalled = false;
            partyMap.OnAllMembersLeft += (sender) =>
            {
                allLeftWasCalled = true;
            };

            character1.PartyManager.Party = null;

            Assert.Null(character1.PartyManager.Party);
            Assert.Null(character2.PartyManager.Party);

            Assert.True(allLeftWasCalled); // No party member visited map, we can delete it.
        }
    }
}
