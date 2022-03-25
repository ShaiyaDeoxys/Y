using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DuelRemoveItemPacket : IPacketDeserializer
    {
        public byte SlotInTradeWindow { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            SlotInTradeWindow = packetStream.Read<byte>();
        }
    }
}
