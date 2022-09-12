using Imgeneus.Game.Monster;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.CharacterTests
{
    public class CharacterTeleportTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to teleport inside one map.")]
        public void Character_Teleport()
        {
            var character = CreateCharacter();
            testMap.LoadPlayer(character);
            Assert.Equal(0, character.PosX);
            Assert.Equal(0, character.PosY);
            Assert.Equal(0, character.PosZ);

            character.TeleportationManager.Teleport(Map.TEST_MAP_ID, 10, 20, 30);
            Assert.Equal(Map.TEST_MAP_ID, character.MapProvider.NextMapId);
            Assert.Equal(10, character.PosX);
            Assert.Equal(20, character.PosY);
            Assert.Equal(30, character.PosZ);
        }

        [Fact]
        [Description("It should be possible to teleport to another map.")]
        public void Character_TeleportAnotherMap()
        {
            var map1 = new Map(
                    1,
                    new MapDefinition(),
                    new Svmap() { MapSize = 100, CellSize = 100 },
                    new List<ObeliskConfiguration>(),
                    new List<BossConfiguration>(),
                    mapLoggerMock.Object,
                    packetFactoryMock.Object,
                    definitionsPreloader.Object,
                    mobFactoryMock.Object,
                    npcFactoryMock.Object,
                    obeliskFactoryMock.Object,
                    timeMock.Object);
            var map2 = new Map(
                    2,
                    new MapDefinition(),
                    new Svmap() { MapSize = 100, CellSize = 100 },
                    new List<ObeliskConfiguration>(),
                    new List<BossConfiguration>(),
                    mapLoggerMock.Object,
                    packetFactoryMock.Object,
                    definitionsPreloader.Object,
                    mobFactoryMock.Object,
                    npcFactoryMock.Object,
                    obeliskFactoryMock.Object,
                    timeMock.Object);

            var character = CreateCharacter();
            map1.LoadPlayer(character);
            Assert.NotNull(map1.GetPlayer(character.Id));

            character.TeleportationManager.Teleport(map2.Id, 10, 20, 30);
            Assert.Null(map1.GetPlayer(character.Id));
        }

        [Fact]
        [Description("Cell id should change if teleport inside one map.")]
        public void TeleportShouldChangeCellId()
        {
            var map = new Map(
                    1,
                    new MapDefinition(),
                    new Svmap() { MapSize = 100, CellSize = 50 },
                    new List<ObeliskConfiguration>(),
                    new List<BossConfiguration>(),
                    mapLoggerMock.Object,
                    packetFactoryMock.Object,
                    definitionsPreloader.Object,
                    mobFactoryMock.Object,
                    npcFactoryMock.Object,
                    obeliskFactoryMock.Object,
                    timeMock.Object);

            Assert.Equal(4, map.Cells.Count);

            var character = CreateCharacter(map);
            Assert.Equal(0, character.CellId);

            character.TeleportationManager.Teleport(1, 90, 0, 90);
            Assert.Equal(3, character.CellId);
        }
    }
}
