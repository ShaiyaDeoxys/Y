using Imgeneus.World.Game.Inventory;
using System.ComponentModel;
using System.Threading.Tasks;
using Xunit;

namespace Imgeneus.World.Tests.ItemTests
{
    public class GenerateAnotherItemTest : BaseTest
    {
        [Fact]
        [Description("Item should generate another item.")]
        public async Task ShouldGenerateAnotherItem()
        {
            var character = CreateCharacter();
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, BoxWithApples.Type, BoxWithApples.TypeId), "");

            var ok = await character.InventoryManager.TryUseItem(1, 0);
            Assert.True(ok);
            Assert.False(character.InventoryManager.InventoryItems.ContainsKey((1, 0))); // used box, should be empty
            Assert.NotNull(character.InventoryManager.InventoryItems[(1, 1)]); // here should be apple
            Assert.Equal(RedApple.Type, character.InventoryManager.InventoryItems[(1, 1)].Type);
        }

        [Fact]
        [Description("When it's full inventory, item should not generate another item.")]
        public async Task FullInventoryShouldNotGenerateAnotherItem()
        {
            var character = CreateCharacter();

            for (var i = 0; i < InventoryManager.MAX_BAG * InventoryManager.MAX_SLOT - 1; i++)
            {
                character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, RedApple.Type, RedApple.TypeId), "");
            }

            var box = character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, BoxWithApples.Type, BoxWithApples.TypeId), "");
            Assert.NotNull(box);

            var ok = await character.InventoryManager.TryUseItem(box.Bag, box.Slot);
            Assert.False(ok);
        }
    }
}
