using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DuelResponsePacket : IPacketDeserializer
    {
        public bool IsDuelApproved { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            IsDuelApproved = packetStream.Read<bool>();
        }
    }
}
