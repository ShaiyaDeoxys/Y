using BinarySerialization;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Serialization
{
    public class GuildUpdateUnit : BaseSerializable
    {
        [FieldOrder(0)]
        public uint Id;

        [FieldOrder(1)]
        public int Points;

        [FieldOrder(2)]
        public byte Rank;

        public GuildUpdateUnit((uint GuildId, int Points, byte Rank) result)
        {
            Id = result.GuildId;
            Points = result.Points;
            Rank = result.Rank;
        }
    }
}
