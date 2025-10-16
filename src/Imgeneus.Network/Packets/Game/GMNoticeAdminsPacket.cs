using Imgeneus.Network.PacketProcessor;
using System.Text;

namespace Imgeneus.Network.Packets.Game
{
    public record GMNoticeAdminsPacket : IPacketDeserializer
    {
        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            var messageLength = packetStream.Read<byte>();
            // Message always ends with an empty character
#if SHAIYA_EG
            Message = packetStream.ReadString(messageLength, Encoding.Unicode);
#else
            Message = packetStream.ReadString(messageLength);
#endif
        }
    }
}