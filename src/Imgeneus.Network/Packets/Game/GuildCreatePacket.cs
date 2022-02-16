using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record GuildCreatePacket : IPacketDeserializer
    {
        public string Name { get; private set; }

        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Name = packetStream.ReadString(25);

#if EP8_V2 || SHAIYA_US || DEBUG
            Message = packetStream.ReadString(25, Encoding.Unicode);
#else
            Message = packetStream.ReadString(25);
#endif
        }
    }
}
