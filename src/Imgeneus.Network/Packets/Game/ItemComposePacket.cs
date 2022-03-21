using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record ItemComposeAbsolutePacket : IPacketDeserializer
    {
        public byte RuneBag { get; private set; }
        public byte RuneSlot { get; private set; }
        public byte ItemBag { get; private set; }
        public byte ItemSlot { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            RuneBag = packetStream.Read<byte>();
            RuneSlot = packetStream.Read<byte>();
            ItemBag = packetStream.Read<byte>();
            ItemSlot = packetStream.Read<byte>();
        }
    }
}
