using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuestQuitPacket : IPacketDeserializer
    {
        public ushort QuestId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            QuestId = packetStream.Read<ushort>();
        }
    }
}
