using System.ComponentModel;
using Imgeneus.World.Game.Inventory;
using Xunit;

namespace Imgeneus.World.Tests.ItemTests
{
    public class TemporaryItemTest : BaseTest
    {
        [Fact]
        [Description("Temporary items should have their expiration date set.")]
        public void TemporaryItem_Expiration()
        {
            var character = CreateCharacter();
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, Nimbus1d.Type, Nimbus1d.TypeId), "");

            character.InventoryManager.InventoryItems.TryGetValue((1, 0), out var item);
            var expectedExpirationTime = item.CreationTime.AddSeconds(Nimbus1d.Duration);

            Assert.Equal(expectedExpirationTime, item.ExpirationTime);
        }
    }
}
