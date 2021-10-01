using Imgeneus.Network.Data;

namespace Imgeneus.Network.Packets.Game
{
    public struct QuitGamePacket : IDeserializedPacket
    {
        public QuitGamePacket(IPacketStream packet)
        {
            // This is empty packet. Needed for server.
        }
    }
}
