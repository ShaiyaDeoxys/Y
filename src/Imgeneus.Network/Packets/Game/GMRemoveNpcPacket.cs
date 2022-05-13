using Imgeneus.Network.PacketProcessor;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.Network.Packets.Game
{
    public record GMRemoveNpcPacket : IPacketDeserializer
    {
        public NpcType Type { get; private set; }

        public ushort TypeId { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Type = (NpcType)packetStream.Read<byte>();
            TypeId = packetStream.Read<ushort>();
            Count = packetStream.Read<byte>();
        }
    }
}
