using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Login
{
    public record OAuthAuthenticationPacket : IPacketDeserializer
    {
        public string key { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            key = packetStream.ReadString(40);
        }
    }
}