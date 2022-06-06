using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Buffs;

namespace Imgeneus.World.Serialization
{
    public class SerializedActiveBuff : BaseSerializable
    {
        [FieldOrder(0)]
        public uint Id;

        [FieldOrder(1)]
        public ushort SkillId;

        [FieldOrder(2)]
        public byte SkillLevel;

        [FieldOrder(3)]
        public int CountDownInSeconds;

        public SerializedActiveBuff(uint id, ushort skillId, byte skillLevel, int countDownInSeconds)
        {
            Id = id;
            SkillId = skillId;
            SkillLevel = skillLevel;
            CountDownInSeconds = countDownInSeconds;
        }
    }
}
