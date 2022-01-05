using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record HandshakePacket : IPacketDeserializer
    {
        public int UserId { get; private set; }

        public Guid SessionId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            UserId = packetStream.Read<int>();
            SessionId = new Guid(packetStream.Read<byte>(16));
        }
    }
}
