using Imgeneus.Network.PacketProcessor;

namespace Imgeneus.Network.Packets.Game
{
    public record AutoStatsSettingsPacket : IPacketDeserializer
    {
        public byte Str { get; private set; }

        public byte Dex { get; private set; }

        public byte Rec { get; private set; }

        public byte Int { get; private set; }

        public byte Wis { get; private set; }

        public byte Luc { get; private set; }

        public void Deserialize(ImgeneusPacket packetStream)
        {
            var unknown = packetStream.Read<uint>(); // probably character id.

            Str = packetStream.Read<byte>();
            Dex = packetStream.Read<byte>();
            Rec = packetStream.Read<byte>();
            Int = packetStream.Read<byte>();
            Luc = packetStream.Read<byte>();
            Wis = packetStream.Read<byte>();
        }

        public void Deconstruct(out byte str, out byte dex, out byte rec, out byte intl, out byte wis, out byte luc)
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
