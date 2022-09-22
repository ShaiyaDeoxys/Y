using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GMStopOnPacket : IPacketDeserializer
    {
        public ushort Time { get; private set; }
        public string Name { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Time = packetStream.Read<ushort>();
            Name = packetStream.ReadString(21);
        }
    }

    public record GMStopOffPacket : IPacketDeserializer
    {
        public string Name { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Name = packetStream.ReadString(21);
        }
    }
}
