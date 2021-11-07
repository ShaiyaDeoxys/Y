using Imgeneus.Network.Data;

namespace Imgeneus.Network.Packets.Game
{
    public struct UseItem2Packet : IDeserializedPacket
    {
        public byte Bag { get; }

        public byte Slot { get; }

        public int TargetId { get; }

        public UseItem2Packet(IPacketStream packet)
        {
            Bag = packet.Read<byte>();
            Slot = packet.Read<byte>();
            TargetId = packet.Read<int>();
        }
    }
}
