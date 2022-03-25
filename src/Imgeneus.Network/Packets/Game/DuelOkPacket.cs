using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DuelOkPacket : IPacketDeserializer
    {
        public byte Result { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Result = packetStream.Read<byte>();
        }
    }
}
