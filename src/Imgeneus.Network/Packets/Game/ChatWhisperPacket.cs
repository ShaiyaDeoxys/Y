using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record ChatWhisperPacket : IPacketDeserializer
    {
        public string TargetName { get;private set; }

        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            TargetName = packetStream.ReadString(21);

#if EP8_V2
            var length0 = packetStream.Read<byte>();
#endif

            var messageLength = packetStream.Read<byte>();

#if EP8_V2 || SHAIYA_US || SHAIYA_US_DEBUG || DEBUG
            Message = packetStream.ReadString(messageLength, Encoding.Unicode);
#else
            Message = packetStream.ReadString(messageLength);
#endif
        }
    }
}
