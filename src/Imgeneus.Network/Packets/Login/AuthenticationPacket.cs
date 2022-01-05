using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Login
{
    public record AuthenticationPacket : IPacketDeserializer
    {
        public string Username { get; private set; }

        public string Unknow { get; private set; }

        public string Password { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Username = packetStream.ReadString(19);
            Unknow = packetStream.ReadString(13);
            Password = packetStream.ReadString(16);
        }
    }
}
