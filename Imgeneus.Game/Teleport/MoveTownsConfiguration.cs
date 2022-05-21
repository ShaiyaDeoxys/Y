using Imgeneus.Core.Helpers;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Teleport
{
    public class MoveTownsConfiguration : IMoveTownsConfiguration
    {
        private const string ConfigFile = "config/MoveTowns.json";

        public static MoveTownsConfiguration LoadFromConfigFile()
        {
            return ConfigurationHelper.Load<MoveTownsConfiguration>(ConfigFile);
        }

        public Dictionary<byte, MoveTownInfo> MoveTowns { get; set; }
    }
}
