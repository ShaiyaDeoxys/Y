using Imgeneus.Network.Data;
using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MoveCharacterPacket : IPacketDeserializer
    {
        /// <summary>
        /// If it's 132 character stopped moving, if it's 129 character is continuously moving.
        /// </summary>
        public MovementType MovementType { get; private set; }

        public ushort Angle { get; private set; }

        public float X { get; private set; }

        public float Y { get; private set; }

        public float Z { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            MovementType = (MovementType)packetStream.Read<byte>();
            Angle = packetStream.Read<ushort>();
            X = packetStream.Read<float>();
            Y = packetStream.Read<float>();
            Z = packetStream.Read<float>();
        }
    }

    public enum MovementType : byte
    {
        Stopped = 132,
        Moving = 129
    }
}
