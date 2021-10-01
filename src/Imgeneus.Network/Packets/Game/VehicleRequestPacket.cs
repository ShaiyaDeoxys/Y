using Imgeneus.Network.Data;

namespace Imgeneus.Network.Packets.Game
{
    public struct VehicleRequestPacket : IDeserializedPacket
    {
        public int CharacterId;

        public VehicleRequestPacket(IPacketStream packet)
        {
            CharacterId = packet.Read<int>();
        }
    }
}
