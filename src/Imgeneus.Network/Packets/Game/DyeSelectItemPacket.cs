using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record DyeSelectItemPacket : IPacketDeserializer
    {
        public byte DyeItemBag { get; private set; }
        public byte DyeItemSlot { get; private set; }
        public byte TargetItemBag { get; private set; }
        public byte TargetItemSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            DyeItemBag = packetStream.Read<byte>();
            DyeItemSlot = packetStream.Read<byte>();
            TargetItemBag = packetStream.Read<byte>();
            TargetItemSlot = packetStream.Read<byte>();
        }
    }
}

