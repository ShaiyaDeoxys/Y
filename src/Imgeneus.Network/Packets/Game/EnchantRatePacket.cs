using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record EnchantRatePacket : IPacketDeserializer
    {
        public byte ItemBag { get; private set; }
        public byte ItemSlot { get; private set; }

        public byte[] LapisiaBag { get; private set; } = new byte[10];
        public byte[] LapisiaSlot { get; private set; } = new byte[10];

        public void Deserialize(ImgeneusPacket packetStream)
        {
            ItemBag = packetStream.ReadByte();
            ItemSlot = packetStream.ReadByte();

            for(var i = 0; i < 10; i++)
                LapisiaBag[i] = packetStream.ReadByte();

            for (var i = 0; i < 10; i++)
                LapisiaSlot[i] = packetStream.ReadByte();
        }
    }
}
