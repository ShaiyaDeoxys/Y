using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record NpcSellItemPacket : IPacketDeserializer
    {
        public byte Bag { get;private set; }

        public byte Slot { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            Count = packetStream.Read<byte>();
        }
    }
}
