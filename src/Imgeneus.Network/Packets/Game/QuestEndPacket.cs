using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuestEndPacket : IPacketDeserializer
    {
        public int NpcId { get; private set; }

        public ushort QuestId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<int>();
            QuestId = packetStream.Read<ushort>();
        }
    }
}
