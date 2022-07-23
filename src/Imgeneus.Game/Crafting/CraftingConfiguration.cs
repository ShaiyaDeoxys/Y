using Imgeneus.Core.Helpers;
using System.Collections.Generic;

namespace Imgeneus.Game.Crafting
{
    public class CraftingConfiguration : ICraftingConfiguration
    {
        private const string ConfigFile = "config/ChaoticSquare.json";

        public static CraftingConfiguration LoadFromConfigFile()
        {
            return ConfigurationHelper.Load<CraftingConfiguration>(ConfigFile);
        }

        public IEnumerable<CraftInfo> SquareItems { get; set; }
    }
}
