using System.Collections.Generic;

namespace Imgeneus.World.Game.Linking
{
    public interface IItemEnchantConfiguration
    {
        /// <summary>
        /// Success % rate.
        /// </summary>
        Dictionary<string, int> LapisianEnchantPercentRate { get; set; }

        /// <summary>
        /// Extra value added to item.
        /// </summary>
        Dictionary<string, int> LapisianEnchantAddValue { get; set; }
    }
}
