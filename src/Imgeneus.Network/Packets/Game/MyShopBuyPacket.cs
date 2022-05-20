using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MyShopBuyPacket : IPacketDeserializer
    {
        public byte Slot { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Slot = packetStream.ReadByte();
            Count = packetStream.ReadByte();
        }
    }
}
