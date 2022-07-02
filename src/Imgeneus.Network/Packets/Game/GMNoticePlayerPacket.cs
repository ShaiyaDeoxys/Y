using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record GMNoticePlayerPacket : IPacketDeserializer
    {
        public string TargetName { get; private set; }
        public short TimeInterval { get; private set; }
        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetName = packetStream.ReadString(21);
            TimeInterval = packetStream.Read<short>();
            var messageLength = packetStream.Read<byte>();

            // Message always ends with an empty character
#if EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG
            Message = packetStream.ReadString(messageLength, Encoding.Unicode);
#else
            Message = packetStream.ReadString(messageLength);
#endif
        }
    }
}