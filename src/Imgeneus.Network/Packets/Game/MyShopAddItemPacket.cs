using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MyShopAddItemPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }

        public byte Slot { get; private set; }

        public byte ShopSlot { get; private set; }

        public uint Price { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            ShopSlot = packetStream.Read<byte>();
            Price = packetStream.Read<uint>();
        }
    }
}
