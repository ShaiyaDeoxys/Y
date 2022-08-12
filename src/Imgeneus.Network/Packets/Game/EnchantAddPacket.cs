using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record EnchantAddPacket : IPacketDeserializer
    {
        public byte LapisiaBag { get; private set; }
        public byte LapisiaSlot { get; private set; }

        public byte ItemBag { get; private set; }
        public byte ItemSlot { get; private set; }

        public byte[] Unknown { get; private set; } = new byte[9];

        public bool IsAutoEnchant { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            LapisiaBag = packetStream.ReadByte();
            LapisiaSlot = packetStream.ReadByte();

            ItemBag = packetStream.ReadByte();
            ItemSlot = packetStream.ReadByte();

            for (var i = 0; i < 9; i++)
                Unknown[i] = packetStream.ReadByte();

            IsAutoEnchant = packetStream.ReadBoolean();
        }
    }
}
