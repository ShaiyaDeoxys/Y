using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record UseItem2Packet : IPacketDeserializer
    {
        public byte Bag { get; private set; }

        public byte Slot { get; private set; }

        public uint TargetId { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            TargetId = packetStream.Read<uint>();
        }
    }
}
