using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record PartySummonAnswerPacket : IPacketDeserializer
    {
        public bool IsDeclined { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            IsDeclined = packetStream.ReadBoolean();
        }
    }
}
