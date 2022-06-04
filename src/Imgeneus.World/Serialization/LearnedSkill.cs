using BinarySerialization;
using Imgeneus.Game.Skills;
using Imgeneus.Network.Serialization;

namespace Imgeneus.World.Serialization
{
    public class LearnedSkill : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Number { get; }

        [FieldOrder(1)]
        public ushort SkillId { get; }

        [FieldOrder(2)]
        public byte SkillLevel { get; }

        public LearnedSkill(Skill skill)
        {
            if (skill is null)
                return;

            Number = skill.Number;
            SkillId = skill.SkillId;
            SkillLevel = skill.SkillLevel;
        }
    }
}
