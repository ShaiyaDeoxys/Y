using Imgeneus.Core.Helpers;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Inventory
{
    public class ItemCreateConfiguration : IItemCreateConfiguration
    {
        private const string ConfigFile = "config/ItemCreate.json";

        public static ItemCreateConfiguration LoadFromConfigFile()
        {
            return ConfigurationHelper.Load<ItemCreateConfiguration>(ConfigFile);
        }

        public Dictionary<ushort, IEnumerable<ItemCreateInfo>> ItemCreateInfo { get; set; }
    }

    public class ItemCreateInfo
    {
        public ushort Grade { get; set; }

        public ushort Weight { get; set; }
    }
}
