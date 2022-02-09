using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record ChatWorldPacket : IPacketDeserializer
    {
        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
#if EP8_V2
            var length0 = packetStream.Read<byte>();
#endif

            var messageLength = packetStream.Read<byte>();

#if EP8_V2 || SHAIYA_US || DEBUG
            Message = packetStream.ReadString(messageLength, Encoding.Unicode);
#else
            Message = packetStream.ReadString(messageLength);
#endif
        }
    }
}
