using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;

namespace Imgeneus.World.Serialization
{
    public class MobMove : BaseSerializable
    {
        [FieldOrder(0)]
        public int GlobalId { get; }

        [FieldOrder(1)]
        public MoveMotion Motion { get; }

        [FieldOrder(2)]
        public float PosX { get; }

        [FieldOrder(3)]
        public float PosZ { get; }

        public MobMove(int senderId, float x, float z, MoveMotion motion)
        {
            GlobalId = senderId;
            Motion = motion;
            PosX = x;
            PosZ = z;
        }
    }
}
