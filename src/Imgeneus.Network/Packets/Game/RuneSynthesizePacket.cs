using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record RuneSynthesizePacket : IPacketDeserializer
    {
        public byte RuneBag { get; private set; }

        public byte RuneSlot { get; private set; }

        public byte VialBag { get; private set; }

        public byte VialSlot { get; private set; }

        public int Unknown1 { get; private set; }

        public int Unknown2 { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            RuneBag = packetStream.Read<byte>();
            RuneSlot = packetStream.Read<byte>();
            VialBag = packetStream.Read<byte>();
            VialSlot = packetStream.Read<byte>();
            Unknown1 = packetStream.Read<int>();
            Unknown2 = packetStream.Read<int>();
        }
    }
}
