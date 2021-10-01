using Imgeneus.Network.Data;

namespace Imgeneus.Network.Packets.Game
{
    public struct VehicleResponsePacket : IDeserializedPacket
    {
        public bool Rejected;

        public VehicleResponsePacket(IPacketStream packet)
        {
            Rejected = packet.Read<bool>();
        }
    }
}
