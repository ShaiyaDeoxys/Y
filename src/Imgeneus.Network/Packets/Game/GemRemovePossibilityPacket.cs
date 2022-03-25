using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record GemRemovePossibilityPacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }
        public bool ShouldRemoveSpecificGem { get; private set; }
        public byte GemPosition { get; private set; }
        public byte HammerBag { get; private set; }
        public byte HammerSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            ShouldRemoveSpecificGem = packetStream.Read<bool>();
            GemPosition = packetStream.Read<byte>();
            HammerBag = packetStream.Read<byte>();
            HammerSlot = packetStream.Read<byte>();
        }
    }
}
