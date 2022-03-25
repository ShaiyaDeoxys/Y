using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record UpdateStatsPacket : IPacketDeserializer
    {
        public ushort Str { get; private set; }
        public ushort Dex { get; private set; }
        public ushort Rec { get; private set; }
        public ushort Int { get; private set; }
        public ushort Wis { get; private set; }
        public ushort Luc { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            Str = packetStream.Read<ushort>();
            Dex = packetStream.Read<ushort>();
            Rec = packetStream.Read<ushort>();
            Int = packetStream.Read<ushort>();
            Wis = packetStream.Read<ushort>();
            Luc = packetStream.Read<ushort>();
        }

        public void Deconstruct(out ushort str, out ushort dex, out ushort rec, out ushort intl, out ushort wis, out ushort luc)
        {
            str = Str;
            dex = Dex;
            rec = Rec;
            intl = Int;
            wis = Wis;
            luc = Luc;
        }
    }
}
