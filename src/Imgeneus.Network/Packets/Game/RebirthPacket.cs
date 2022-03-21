using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RebirthPacket : IPacketDeserializer
    {
        public byte RebirthType;

        public void Deserialize(ImgeneusPacket packetStream)
        {
            RebirthType = packetStream.Read<byte>();
        }
    }
}
