using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record FriendRequestPacket : IPacketDeserializer
    {
        public string CharacterName { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CharacterName = packetStream.ReadString(21);
        }
    }
}
