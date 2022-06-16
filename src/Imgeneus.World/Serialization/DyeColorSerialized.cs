using BinarySerialization;

namespace Imgeneus.World.Serialization
{
    public class DyeColorSerialized
    {
        [FieldOrder(0)]
        public byte Alpha;

        [FieldOrder(1)]
        public byte R;

        [FieldOrder(2)]
        public byte G;

        [FieldOrder(3)]
        public byte B;

        public DyeColorSerialized()
        {
            Alpha = 0;
            R = 0;
            G = 0;
            B = 0;
        }

        public DyeColorSerialized(byte a, byte r, byte g, byte b, byte s = 0)
        {
            Alpha = a;
            R = r;

#if SHAIYA_US || DEBUG || SHAIYA_US_DEBUG
            if (r < 20) // Pay attention in shaiya us there is bug, that saturation is saved as red color.
                r = s;
#endif
            G = g;
            B = b;
        }
    }
}
