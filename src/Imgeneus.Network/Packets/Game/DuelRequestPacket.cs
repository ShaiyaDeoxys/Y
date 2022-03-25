using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DuelRequestPacket : IPacketDeserializer
    {
        public int DuelToWhomId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            DuelToWhomId = packetStream.Read<int>();
        }
    }
}
