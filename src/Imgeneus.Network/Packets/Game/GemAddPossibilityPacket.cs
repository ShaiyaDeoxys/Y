using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GemAddPossibilityPacket : IPacketDeserializer
    {
        public byte GemBag { get; private set; }
        public byte GemSlot { get; private set; }
        public byte DestinationBag { get; private set; }
        public byte DestinationSlot { get; private set; }
        public byte HammerBag { get; private set; }
        public byte HammerSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            GemBag = packetStream.Read<byte>();
            GemSlot = packetStream.Read<byte>();
            DestinationBag = packetStream.Read<byte>();
            DestinationSlot = packetStream.Read<byte>();
            HammerBag = packetStream.Read<byte>();
            HammerSlot = packetStream.Read<byte>();
        }
    }
}
