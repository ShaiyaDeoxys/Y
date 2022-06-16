using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record MyShopStartPacket : IPacketDeserializer
    {
        public byte Length { get; private set; }

        public string Name { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Length = packetStream.Read<byte>();

#if EP8_V2 || SHAIYA_US || DEBUG || SHAIYA_US_DEBUG
            Name = packetStream.ReadString(Length, Encoding.Unicode);
#else
            Name = packetStream.ReadString(Length);
#endif
        }
    }
}
