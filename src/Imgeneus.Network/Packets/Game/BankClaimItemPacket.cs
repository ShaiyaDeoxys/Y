using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record BankClaimItemPacket : IPacketDeserializer
    {
        public byte Slot { get; private set; }
        public int Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Slot = packetStream.Read<byte>();
            Unknown1 = packetStream.Read<int>();
            Unknown2 = packetStream.Read<int>();
        }
    }
}
