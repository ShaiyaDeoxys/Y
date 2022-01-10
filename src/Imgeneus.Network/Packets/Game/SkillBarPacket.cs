using Imgeneus.Network.PacketProcessor;
using Imgeneus.World.Game.Player;

namespace Imgeneus.Network.Packets.Game
{
    public record SkillBarPacket : IPacketDeserializer
    {
        public QuickSkillBarItem[] QuickItems;

        public void Deserialize(ImgeneusPacket packetStream)
        {
            var count = packetStream.Read<byte>();
            var unknown = packetStream.Read<int>();

            QuickItems = new QuickSkillBarItem[count - 1];

            for (var i = 0; i < count - 1; i++)
            {
                var bar = packetStream.Read<byte>();
                var slot = packetStream.Read<byte>();
                var bag = packetStream.Read<byte>();
                var number = packetStream.Read<ushort>();
                var unknown2 = packetStream.Read<int>(); // cooldown?

                QuickItems[i] = new QuickSkillBarItem(bar, slot, bag, number);
            }

            // There are still 5 bytes after all. But they are always the same.
            // These are 21, 1, 255, 0, 0. I leave it unimplemented, since i have no idea why they are needed.
        }
    }
}
