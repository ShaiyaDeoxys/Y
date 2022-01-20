using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GemAddPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }
        public byte DestinationBag { get; private set; }
        public byte DestinationSlot { get; private set; }
        public byte HammerBag { get; private set; }
        public byte HammerSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            DestinationBag = packetStream.Read<byte>();
            DestinationSlot = packetStream.Read<byte>();
            HammerBag = packetStream.Read<byte>();
            HammerSlot = packetStream.Read<byte>();
        }
    }
}
