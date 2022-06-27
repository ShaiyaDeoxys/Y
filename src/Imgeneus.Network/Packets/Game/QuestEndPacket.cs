using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuestEndPacket : IPacketDeserializer
    {
        public uint NpcId { get; private set; }

        public short QuestId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<uint>();
            QuestId = packetStream.Read<short>();
        }
    }
}
