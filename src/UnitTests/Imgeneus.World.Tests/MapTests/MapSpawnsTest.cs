using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Moq;
using Parsec.Shaiya.Common;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.MapTests
{
    public class MapSpawnsTest : BaseTest
    {
        [Theory]
        [Description("It should be possible to find the nearest spawn.")]
        [InlineData(2, 2, CountryType.Light, 0, 1, 0, 1)]
        [InlineData(7, 8, CountryType.Light, 9, 10, 9, 10)]
        [InlineData(1, 1, CountryType.Dark, 9, 10, 0, 1)]
        public void MapSpawns_FindsNearest(float x, float z, CountryType fraction, float expectedMinX, float expectedMaxX, float expectedMinZ, float expectedMaxZ)
        {
            //var spawn1 = new Mock<Spawn>();
            //spawn1.SetupGet(x => x.Faction)
            //      .Returns(1);
            //spawn1.SetupGet(x => x.Area)
            //      .Returns(new BoundingBox() { 
            //          LowerLimit = new Vector3() { 
            //              X = 0,
            //              Z = 0
            //          },
            //          UpperLimit = new Vector3() {
            //              X = 1,
            //              Z = 1
            //          }
            //      });

            //var spawn2 = new Mock<Spawn>();
            //spawn2.SetupGet(x => x.Faction)
            //      .Returns(1);
            //spawn2.SetupGet(x => x.Area)
            //      .Returns(new BoundingBox()
            //      {
            //          LowerLimit = new Vector3()
            //          {
            //              X = 9,
            //              Z = 9
            //          },
            //          UpperLimit = new Vector3()
            //          {
            //              X = 10,
            //              Z = 10
            //          }
            //      });

            //var spawn3 = new Mock<Spawn>();
            //spawn3.SetupGet(x => x.Faction)
            //      .Returns(1);
            //spawn3.SetupGet(x => x.Area)
            //      .Returns(new BoundingBox()
            //      {
            //          LowerLimit = new Vector3()
            //          {
            //              X = 9,
            //              Z = 0
            //          },
            //          UpperLimit = new Vector3()
            //          {
            //              X = 10,
            //              Z = 1
            //          }
            //      });

            //var mapConfig = new Mock<Svmap>();
            //mapConfig.SetupGet(x => x.MapSize)
            //         .Returns(10);
            //mapConfig.SetupGet(x => x.CellSize)
            //         .Returns(1);
            //mapConfig.SetupGet(x => x.Spawns)
            //         .Returns(new List<Spawn>()
            //         {
            //             spawn1.Object,
            //             spawn2.Object,
            //             spawn3.Object
            //         });

            //var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig.Object, new List<ObeliskConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, databasePreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            //var spawn = map.GetNearestSpawn(x, 0, z, fraction);
            //Assert.True(spawn.X >= expectedMinX && spawn.X <= expectedMaxX);
            //Assert.True(spawn.Z >= expectedMinZ && spawn.Z <= expectedMaxZ);
        }
    }
}
