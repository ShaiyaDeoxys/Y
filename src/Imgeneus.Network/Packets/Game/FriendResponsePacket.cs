using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record FriendResponsePacket : IPacketDeserializer
    {
        public bool Accepted { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Accepted = packetStream.Read<bool>();
        }
    }
}
