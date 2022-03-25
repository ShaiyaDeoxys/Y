using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMWarningPacket : IPacketDeserializer
    {
        public string Name { get; private set; }
        public string Message { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Name = packetStream.ReadString(21);

            var messageLength = packetStream.Read<byte>();
            Message = packetStream.ReadString(messageLength);
        }
    }
}