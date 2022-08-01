using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MarketAddFavoritePacket : IPacketDeserializer
    {
        public uint MarketId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MarketId = packetStream.Read<uint>();
        }
    }
}
