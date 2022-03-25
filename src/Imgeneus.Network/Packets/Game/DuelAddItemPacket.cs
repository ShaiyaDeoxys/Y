using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DuelAddItemPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }

        public byte Slot { get; private set; }

        public byte Quantity { get; private set; }

        public byte SlotInTradeWindow { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            Quantity = packetStream.Read<byte>();
            SlotInTradeWindow = packetStream.Read<byte>();
        }
    }
}
