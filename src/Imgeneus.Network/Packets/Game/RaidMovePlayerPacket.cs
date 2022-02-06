using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RaidMovePlayerPacket : IPacketDeserializer
    {
        public int SourceIndex { get; private set; }

        public int DestinationIndex { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            SourceIndex = packetStream.Read<int>();
            DestinationIndex = packetStream.Read<int>();
        }
    }
}
