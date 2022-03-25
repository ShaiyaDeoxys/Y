using Imgeneus.Database.Entities;
using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record AccountFractionPacket : IPacketDeserializer
    {
        public Fraction Fraction { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Fraction = (Fraction)packetStream.Read<byte>();
        }
    }
}
