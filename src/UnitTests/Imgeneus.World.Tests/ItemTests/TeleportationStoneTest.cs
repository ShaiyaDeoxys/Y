using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Zone;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

namespace Imgeneus.World.Tests.ItemTests
{
    public class TeleportationStoneTest : BaseTest
    {
        [Fact]
        [Description("It should not be possible to teleport, if there is no saved place index.")]
        public async Task NotPossibleToTeleportIfNoSavedPlaceIndex()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, TeleportationStone.Type, TeleportationStone.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 0);

            Assert.Equal(0, character.TeleportationManager.CastingPosition.MapId);
        }

        [Fact]
        [Description("It should not be possible to teleport, if there is no saved place.")]
        public async Task NotPossibleToTeleportIfNoSavedPlace()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, TeleportationStone.Type, TeleportationStone.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 0, 1);

            Assert.Equal(0, character.TeleportationManager.CastingPosition.MapId);
        }

        [Fact]
        [Description("It should not be possible to teleport, if saved place index > max saved places.")]
        public async Task NotPossibleToTeleportIfSavedPlaceIndexMoreThanMax()
        {
            var character = CreateCharacter();

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, TeleportationStone.Type, TeleportationStone.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 0, 2);

            Assert.Equal(0, character.TeleportationManager.CastingPosition.MapId);
        }

        [Fact]
        [Description("It should be possible to teleport.")]
        public async Task PossibleToTeleport()
        {
            var character = CreateCharacter();

            character.TeleportationManager.TrySavePosition(1, Map.TEST_MAP_ID, 0, 0, 0);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, TeleportationStone.Type, TeleportationStone.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 0, 1);

            Assert.Equal(Map.TEST_MAP_ID, character.TeleportationManager.CastingPosition.MapId);
        }

        [Fact]
        [Description("Using 'Blue Dragon Charm' item should increase max teleport places count.")]
        public async Task BlueDragonCharmIncreasesMaxTeleportPlaces()
        {
            var character = CreateCharacter();

            character.TeleportationManager.TrySavePosition(1, Map.TEST_MAP_ID, 0, 0, 0);
            character.TeleportationManager.TrySavePosition(2, Map.TEST_MAP_ID, 10, 10, 10);
            Assert.Equal(1, character.TeleportationManager.SavedPositions.Count);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, TeleportationStone.Type, TeleportationStone.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 0, 2);
            Assert.Equal(0, character.TeleportationManager.CastingPosition.MapId);

            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, BlueDragonCharm.Type, BlueDragonCharm.TypeId), "");
            await character.InventoryManager.TryUseItem(1, 1);

            Assert.Equal(2, character.TeleportationManager.MaxSavedPoints);
            character.TeleportationManager.TrySavePosition(1, Map.TEST_MAP_ID, 0, 0, 0);
            character.TeleportationManager.TrySavePosition(2, Map.TEST_MAP_ID, 10, 10, 10);
            Assert.Equal(2, character.TeleportationManager.SavedPositions.Count);

            await character.InventoryManager.TryUseItem(1, 0, 2);
            Assert.Equal(Map.TEST_MAP_ID, character.TeleportationManager.CastingPosition.MapId);
            Assert.Equal(10, character.TeleportationManager.CastingPosition.X);
            Assert.Equal(10, character.TeleportationManager.CastingPosition.Y);
            Assert.Equal(10, character.TeleportationManager.CastingPosition.Z);
        }
    }
}
