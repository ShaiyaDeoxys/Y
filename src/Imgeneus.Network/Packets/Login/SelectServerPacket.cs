using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Login
{
    public record SelectServerPacket : IPacketDeserializer
    {
        public byte WorldId { get; private set; }

        public int BuildClient { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            WorldId = packetStream.Read<byte>();
            BuildClient = packetStream.Read<int>();
        }
    }
}
