using System.ComponentModel;
using System.Threading.Tasks;
using Imgeneus.World.Game.Inventory;
using Xunit;

namespace Imgeneus.World.Tests.AccountTests
{
    public class BankTest : BaseTest
    {
        [Fact]
        [Description("Characters should receive bank claimed items in the first available inventory slot.")]
        public async Task Bank_Test()
        {
            var character = CreateCharacter();
            character.InventoryManager.AddItem(new Item(definitionsPreloader.Object, enchantConfig.Object, itemCreateConfig.Object, FireSword.Type, FireSword.TypeId), "");

            character.BankManager.AddBankItem(WaterArmor.Type, WaterArmor.TypeId, 1);
            Assert.NotNull(character.BankManager.BankItems[0]);

            var claimedItem = character.BankManager.TryClaimBankItem(0);
            Assert.NotNull(claimedItem);

            // Item should be in the 2nd slot (slot = 1)
            character.InventoryManager.InventoryItems.TryGetValue((1, 1), out var item);
            Assert.NotNull(item);
            Assert.Equal(claimedItem, item);
        }
    }
}
