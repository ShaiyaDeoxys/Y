using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TargetPlayerGetHPPacket : IPacketDeserializer
    {
        public int TargetId { get; private set; }
        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetId = packetStream.Read<int>();
        }
    }
}
