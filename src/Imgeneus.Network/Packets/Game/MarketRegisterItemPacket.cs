using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MarketRegisterItemPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }
        public byte Count { get; private set; }
        public byte MarketType { get; private set; }
        public uint MinMoney { get; private set; }
        public uint DirectMoney { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.ReadByte();
            Slot = packetStream.ReadByte();
            Count = packetStream.ReadByte();
            MarketType = packetStream.ReadByte();
            MinMoney = packetStream.Read<uint>();
            DirectMoney = packetStream.Read<uint>();
        }
    }
}
