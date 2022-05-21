using System.Collections.Generic;

namespace Imgeneus.World.Game.Inventory
{
    public interface IItemCreateConfiguration
    {
        Dictionary<ushort, IEnumerable<ItemCreateInfo>> ItemCreateInfo { get; }
    }
}
