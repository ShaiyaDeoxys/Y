using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record CharacterEnteredPortalPacket : IPacketDeserializer
    {
        public byte PortalId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            PortalId = packetStream.Read<byte>();
        }
    }
}
