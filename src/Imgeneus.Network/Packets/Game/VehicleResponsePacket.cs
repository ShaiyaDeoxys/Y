using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record VehicleResponsePacket : IPacketDeserializer
    {
        public bool Rejected { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Rejected = packetStream.Read<bool>();
        }
    }
}
