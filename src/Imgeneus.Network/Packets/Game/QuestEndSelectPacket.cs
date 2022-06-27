using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuestEndSelectPacket : IPacketDeserializer
    {
        public uint NpcId { get; private set; }

        public short QuestId { get; private set; }

        public byte Index { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<uint>();
            QuestId = packetStream.Read<short>();
            Index = packetStream.Read<byte>();
        }
    }
}
