using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record NpcBuyItemPacket : IPacketDeserializer
    {
        public int NpcId { get; private set; }

        public byte ItemIndex { get; private set; }

        public byte Count { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            NpcId = packetStream.Read<int>();
            ItemIndex = packetStream.Read<byte>();
            Count = packetStream.Read<byte>();
        }
    }
}
