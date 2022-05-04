using Imgeneus.World.Game.Country;

namespace Imgeneus.World.Game.Teleport
{
    public class MoveTownInfo
    {
        public ushort MapId { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public CountryType Country { get; set; }
    }
}
