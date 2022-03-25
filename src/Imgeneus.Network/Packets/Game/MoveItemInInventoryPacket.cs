using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MoveItemInInventoryPacket : IPacketDeserializer
    {
        public byte CurrentBag { get; private set; }

        public byte CurrentSlot { get; private set; }

        public byte DestinationBag { get; private set; }

        public byte DestinationSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            CurrentBag = packetStream.Read<byte>();
            CurrentSlot = packetStream.Read<byte>();
            DestinationBag = packetStream.Read<byte>();
            DestinationSlot = packetStream.Read<byte>();
        }
    }
}
