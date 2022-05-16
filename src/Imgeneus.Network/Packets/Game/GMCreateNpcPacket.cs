using Imgeneus.Network.PacketProcessor;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.Network.Packets.Game
{
    public record GMCreateNpcPacket : IPacketDeserializer
    {
        public NpcType Type { get; private set; }

        public short TypeId { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Type = (NpcType)packetStream.Read<byte>();
            TypeId = packetStream.Read<short>();
            Count = packetStream.Read<byte>();
        }
    }
}
