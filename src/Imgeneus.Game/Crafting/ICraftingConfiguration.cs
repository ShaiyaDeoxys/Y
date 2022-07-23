using System.Collections.Generic;

namespace Imgeneus.Game.Crafting
{
    public interface ICraftingConfiguration
    {
        IEnumerable<CraftInfo> SquareItems { get; set; }
    }
}
