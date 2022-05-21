using Imgeneus.World.Game.Inventory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Dyeing
{
    public interface IDyeingManager
    {
        /// <summary>
        /// Rerolls random colors.
        /// </summary>
        void Reroll();

        /// <summary>
        /// Available colors for dyeing.
        /// </summary>
        List<DyeColor> AvailableColors { get; }

        /// <summary>
        /// Selects item for dyeing
        /// </summary>
        /// <returns>true if item can be painted, otherwise false</returns>
        bool SelectItem(byte dyeItemBag, byte dyeItemSlot, byte targetItemBag, byte targetItemSlot);

        /// <summary>
        /// Paints item to random color.
        /// </summary>
        Task<(bool Ok, DyeColor Color)> Dye(byte dyeItemBag, byte dyeItemSlot, byte targetItemBag, byte targetItemSlot);
    }
}
