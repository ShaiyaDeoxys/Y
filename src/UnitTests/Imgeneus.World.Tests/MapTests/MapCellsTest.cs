using Imgeneus.Game.Monster;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Moq;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using Npc = Imgeneus.World.Game.NPCs.Npc;

namespace Imgeneus.World.Tests.MapTests
{
    public class MapCellsTest : BaseTest
    {
        [Fact]
        [Description("Map should calculate number of its' cells. I.e. number of rows and columns.")]
        public void MapCells_RowColumnNumber_1()
        {
            var mapConfig = new Svmap()
            {
                MapSize = 2000,
                CellSize = 100
            };

            var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig, new List<ObeliskConfiguration>(), new List<BossConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, definitionsPreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            Assert.Equal(20, map.Rows);
            Assert.Equal(20, map.Columns);
        }

        [Fact]
        [Description("Map should calculate number of its' cells. I.e. number of rows and columns.")]
        public void MapCells_RowColumnNumber_2()
        {
            var mapConfig = new Svmap()
            {
                MapSize = 2048,
                CellSize = 100
            };

            var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig, new List<ObeliskConfiguration>(), new List<BossConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, definitionsPreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            Assert.Equal(21, map.Rows);
            Assert.Equal(21, map.Columns);
        }

        [Fact]
        [Description("There can be only 1 cell for the whole map")]
        public void MapCells_OneCell()
        {
            var mapConfig = new Svmap()
            {
                MapSize = 100,
                CellSize = 100
            };
            var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig, new List<ObeliskConfiguration>(), new List<BossConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, definitionsPreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            Assert.Single(map.Cells);
        }

        [Theory]
        [Description("It should be possible to get by coordinates in what cell map member is situated.")]
        [InlineData(0, 0, 0)]
        [InlineData(1, 0, 0)]
        [InlineData(0, 1, 0)]
        [InlineData(90, 70, 0)]
        [InlineData(99, 99, 0)]
        [InlineData(101, 70, 1)]
        [InlineData(101, 99, 1)]
        [InlineData(250, 50, 2)]
        [InlineData(350, 350, 36)]
        [InlineData(101, 101, 12)]
        [InlineData(999, 1001, 119)]
        [InlineData(999, 1002, 119)]
        [InlineData(1000, 1000, 120)]
        [InlineData(1002, 1002, 120)]
        public void MapCells_GetIndex(float x, float z, int expectedCellIndex)
        {
            var mapConfig = new Svmap()
            {
                MapSize = 1002,
                CellSize = 100
            };
            var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig, new List<ObeliskConfiguration>(), new List<BossConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, definitionsPreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            var character = CreateCharacter();
            character.MovementManager.PosX = x;
            character.MovementManager.PosZ = z;

            Assert.Equal(expectedCellIndex, map.GetCellIndex(character));
        }

        [Theory]
        [Description("It should be possible to get cell neighbors indexes.")]
        [InlineData(0, new int[] { 1, 4, 5 })]
        [InlineData(1, new int[] { 0, 2, 4, 5, 6 })]
        [InlineData(2, new int[] { 1, 3, 5, 6, 7 })]
        [InlineData(3, new int[] { 2, 6, 7 })]
        [InlineData(4, new int[] { 0, 1, 5, 8, 9 })]
        [InlineData(5, new int[] { 0, 1, 2, 4, 6, 8, 9, 10 })]
        [InlineData(6, new int[] { 1, 2, 3, 5, 7, 9, 10, 11 })]
        [InlineData(7, new int[] { 2, 3, 6, 10, 11 })]
        [InlineData(8, new int[] { 4, 5, 9, 12, 13 })]
        [InlineData(9, new int[] { 4, 5, 6, 8, 10, 12, 13, 14 })]
        [InlineData(10, new int[] { 5, 6, 7, 9, 11, 13, 14, 15 })]
        [InlineData(11, new int[] { 6, 7, 10, 14, 15 })]
        [InlineData(12, new int[] { 8, 9, 13 })]
        [InlineData(13, new int[] { 12, 8, 9, 10, 14 })]
        [InlineData(14, new int[] { 9, 10, 11, 13, 15 })]
        [InlineData(15, new int[] { 14, 10, 11 })]
        public void MapCells_GetNeighborCellIndexes(int cellId, int[] expectedNeigbors)
        {
            var mapConfig = new Svmap()
            {
                MapSize = 4,
                CellSize = 1
            };

            var map = new Map(Map.TEST_MAP_ID, new MapDefinition(), mapConfig, new List<ObeliskConfiguration>(), new List<BossConfiguration>(), mapLoggerMock.Object, packetFactoryMock.Object, definitionsPreloader.Object, mobFactoryMock.Object, npcFactoryMock.Object, obeliskFactoryMock.Object, timeMock.Object);
            Assert.Equal(expectedNeigbors.OrderBy(i => i), map.GetNeighborCellIndexes(cellId).ToArray());
        }

        [Fact]
        [Description("Mobs are removed as soon as the map cell is destroyed.")]
        public void MapCell_Mob_Dispose()
        {
            var map = testMap;
            var cell = new MapCell(0, new List<int>(), map);
            cell.AddMob(CreateMob(Wolf.Id, map));

            Assert.NotEmpty(cell.GetAllMobs(false));

            cell.Dispose();
            Assert.Empty(cell.GetAllMobs(false));
        }

        [Fact]
        [Description("NPCs are removed as soon as the map cell is destroyed.")]
        public void MapCell_Npc_Dispose()
        {
            var map = testMap;
            var cell = new MapCell(0, new List<int>(), map);
            cell.AddNPC(new Npc(npcLoggerMock.Object, WeaponMerchant, new List<(float X, float Y, float Z, ushort Angle)>() { (0, 0, 0, 0) }, new Mock<IMovementManager>().Object, new Mock<ICountryProvider>().Object, new Mock<IMapProvider>().Object));

            Assert.NotEmpty(cell.GetAllNPCs(false));

            cell.Dispose();
            Assert.Empty(cell.GetAllNPCs(false));
        }

        [Fact]
        [Description("Items are removed as soon as the map cell is destroyed.")]
        public void MapCell_Item_Dispose()
        {
            var map = testMap;
            var cell = new MapCell(0, new List<int>(), map);
            cell.AddItem(new MapItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), null, 0, 0, 0));

            Assert.NotEmpty(cell.GetAllItems(false));

            cell.Dispose();
            Assert.Empty(cell.GetAllItems(false));
        }
    }
}
