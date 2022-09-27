using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Zone;
using System.ComponentModel;
using Xunit;

namespace Imgeneus.World.Tests.MapTests
{
    public class MapItemsTest : BaseTest
    {
        [Fact]
        [Description("It should be possible to add/remove item to/from map.")]
        public void AddAndRemoveItemToMapTest()
        {
            var map = testMap;
            var character = CreateCharacter(map);
            var mapItem = new MapItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), null, 1, 1, 1);

            map.AddItem(mapItem);
            Assert.NotNull(map.GetItem(mapItem.Id, character).Item);

            map.RemoveItem(character.CellId, mapItem.Id);
            Assert.Null(map.GetItem(mapItem.Id, character).Item);
        }
    }
}
