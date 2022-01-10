using Imgeneus.Database.Constants;
using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MotionPacket : IPacketDeserializer
    {
        public Motion Motion { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Motion = (Motion)packetStream.Read<byte>();
        }
    }
}
