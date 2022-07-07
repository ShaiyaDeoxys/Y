using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Movement;

namespace Imgeneus.World.Serialization
{
    public class CharacterMove : BaseSerializable
    {
        [FieldOrder(0)]
        public uint CharId { get; }

        [FieldOrder(1)]
        public ushort Angle { get; }

        [FieldOrder(2)]
        public MoveMotion Motion { get; }

        [FieldOrder(3)]
        public float PosX { get; }

        [FieldOrder(4)]
        public float PosY { get; }

        [FieldOrder(5)]
        public float PosZ { get; }

        public CharacterMove(uint characterId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            CharId = characterId;
            Motion = motion;
            Angle = a;
            PosX = x;
            PosY = y;
            PosZ = z;
        }
    }
}
