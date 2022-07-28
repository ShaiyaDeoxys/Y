using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MarketSearchSectionPacket : IPacketDeserializer
    {
        public MarketSearchAction Action { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Action = (MarketSearchAction)packetStream.ReadByte();
        }
    }

    public enum MarketSearchAction: byte
    {
        MovePrev = 1,
        MoveNext = 2
    }
}
