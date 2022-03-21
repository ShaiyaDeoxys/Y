using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMGetItemPacket : IPacketDeserializer
    {
        public byte Type { get; private set; }

        public byte TypeId { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Type = packetStream.Read<byte>();
            TypeId = packetStream.Read<byte>();
            Count = packetStream.Read<byte>();
        }
    }
}
