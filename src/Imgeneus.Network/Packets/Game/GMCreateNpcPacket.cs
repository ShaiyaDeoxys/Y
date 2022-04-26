using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMCreateNpcPacket : IPacketDeserializer
    {
        public byte Type { get; private set; }

        public short TypeId { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Type = packetStream.Read<byte>();
            TypeId = packetStream.Read<short>();
            Count = packetStream.Read<byte>();
        }
    }
}
