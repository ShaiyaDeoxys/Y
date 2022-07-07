using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MoveCharacterPacket : IPacketDeserializer
    {
        public ushort Angle { get; private set; }

        public byte MoveMotion { get; private set; }

        public float X { get; private set; }

        public float Y { get; private set; }

        public float Z { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Angle = packetStream.Read<ushort>();
            MoveMotion = packetStream.Read<byte>();
            X = packetStream.Read<float>();
            Y = packetStream.Read<float>();
            Z = packetStream.Read<float>();
        }
    }
}
