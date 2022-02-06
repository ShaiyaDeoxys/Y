using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record TradeAddMoneyPacket : IPacketDeserializer
    {
        public uint Money { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Money = packetStream.Read<uint>();
        }
    }
}
