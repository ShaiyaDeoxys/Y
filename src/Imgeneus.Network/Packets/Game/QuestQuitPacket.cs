using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record QuestQuitPacket : IPacketDeserializer
    {
        public short QuestId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            QuestId = packetStream.Read<short>();
        }
    }
}
