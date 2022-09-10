using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record MarketSearchFirstPacket : IPacketDeserializer
    {
        public MarketSearchCountry SearchCountry { get; private set; }

        public byte MinLevel { get; private set; }

        public byte MaxLevel { get; private set; }

        public MarketItemType MarketItemType { get; private set; }

        public byte Grade { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            SearchCountry = (MarketSearchCountry)packetStream.Read<byte>();
            MinLevel = packetStream.Read<byte>();
            MaxLevel = packetStream.Read<byte>();
            MarketItemType = (MarketItemType)packetStream.Read<byte>();
            Grade = packetStream.Read<byte>();
        }
    }

    public enum MarketSearchCountry: byte
    {
        Both = 0,
        Light = 1,
        Dark = 2
    }

    public enum MarketItemType: byte
    {
        None = 0,
        TwoHandedWeapon = 1,
        SharpenWeapon = 2,
        DualWeapon = 3,
        Spear = 4,
        HeavyWeapon = 5,
        LogWeapon = 6,
        DaggerWeapon = 7,
        Staff = 8,
        Bow = 9,
        Projectile = 10,
        Helmet = 11,
        UpperArmor = 12,
        LowerArmor = 13,
        Gloves = 14,
        Shoes = 15,
        Coat = 16,
        Shield = 17,
        Costume = 18,
        Necklace= 19,
        Ring = 20,
        Bracelet = 21,
        Lapis = 22,
        Lapisia = 23,
        OtherItems = 24,
        Mount= 25,
        HighQualityConsumableItem = 26
    }
}
