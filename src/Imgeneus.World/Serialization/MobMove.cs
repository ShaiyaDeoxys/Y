using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Movement;

namespace Imgeneus.World.Serialization
{
    public class MobMove : BaseSerializable
    {
        [FieldOrder(0)]
        public uint GlobalId { get; }

        [FieldOrder(1)]
        public MoveMotion Motion { get; }

        [FieldOrder(2)]
        public float PosX { get; }

        [FieldOrder(3)]
        public float PosZ { get; }

        public MobMove(uint senderId, float x, float z, MoveMotion motion)
        {
            GlobalId = senderId;
            Motion = motion;
            PosX = x;
            PosZ = z;
        }
    }
}
