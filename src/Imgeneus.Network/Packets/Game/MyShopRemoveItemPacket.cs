using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MyShopRemoveItemPacket : IPacketDeserializer
    {
        public byte ShopSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            ShopSlot = packetStream.Read<byte>();
        }
    }
}
