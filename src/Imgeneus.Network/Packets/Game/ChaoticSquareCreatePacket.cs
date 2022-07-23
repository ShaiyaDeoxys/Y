using Imgeneus.Network.PacketProcessor;
using System;

namespace Imgeneus.Network.Packets.Game
{
    public record ChaoticSquareCreatePacket : IPacketDeserializer
    {
        public byte Bag { get; private set; }
        public byte Slot { get; private set; }
        public int Unknown { get; private set; }
        public int Index { get; private set; }
        public byte HammerBag { get; private set; }
        public byte HammerSlot { get; private set; }


        public void Deserialize(ImgeneusPacket packetStream)
        {
            Bag = packetStream.Read<byte>();
            Slot = packetStream.Read<byte>();
            Unknown = packetStream.Read<int>();
            Index = packetStream.Read<int>();
            HammerBag = packetStream.Read<byte>();
            HammerSlot = packetStream.Read<byte>();
        }
    }
}
