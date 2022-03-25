using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMTeleportToPlayerPacket : IPacketDeserializer
    {
        public string Name { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Name = packetStream.ReadString(21);
        }
    }
}
