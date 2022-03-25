using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TradeDecidePacket : IPacketDeserializer
    {
        public bool IsDecided { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            IsDecided = packetStream.Read<bool>();
        }
    }
}
