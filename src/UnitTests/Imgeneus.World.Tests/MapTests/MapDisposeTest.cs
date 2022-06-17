using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.MapTests
{
    public class MapDisposeTest : BaseTest
    {
        [Fact]
        [Description("Cells are cleared, when the map is destroyed.")]
        public void MapDispose_Cells()
        {
            var map = testMap;
            Assert.NotEmpty(map.Cells);

            map.Dispose();
            Assert.Empty(map.Cells);
        }

        [Fact]
        [Description("Mobs are cleared, when the map is destroyed.")]
        public void MapDispose_Mobs()
        {
            var map = testMap;
            var mob1 = CreateMob(Wolf.Id, map);
            var mob2 = CreateMob(Wolf.Id, map);
            var mob3 = CreateMob(Wolf.Id, map);
            var mob4 = CreateMob(Wolf.Id, map);
            var mob5 = CreateMob(Wolf.Id, map);

            Assert.NotNull(map.GetMob(0, mob1.Id));
            Assert.NotNull(map.GetMob(0, mob2.Id));
            Assert.NotNull(map.GetMob(0, mob3.Id));
            Assert.NotNull(map.GetMob(0, mob4.Id));
            Assert.NotNull(map.GetMob(0, mob5.Id));

            map.Dispose();

            Assert.Throws<ObjectDisposedException>(() => map.GetMob(0, mob1.Id));
            Assert.Throws<ObjectDisposedException>(() => map.GetMob(0, mob2.Id));
            Assert.Throws<ObjectDisposedException>(() => map.GetMob(0, mob3.Id));
            Assert.Throws<ObjectDisposedException>(() => map.GetMob(0, mob4.Id));
            Assert.Throws<ObjectDisposedException>(() => map.GetMob(0, mob5.Id));
        }

        [Fact]
        [Description("Map can not be disposed, if at least one player is connected to this map.")]
        public void MapDispose_NotAllowed()
        {
            var map = testMap;
            var character = CreateCharacter(map);

            Assert.NotNull(map.GetPlayer(character.Id));

            Assert.Throws<Exception>(() => map.Dispose());
        }
    }
}
