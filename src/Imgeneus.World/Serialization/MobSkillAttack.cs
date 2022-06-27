using BinarySerialization;
using Imgeneus.Game.Skills;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Monster;

namespace Imgeneus.World.Serialization
{
    public class MobSkillAttack : BaseSerializable
    {
        [FieldOrder(0)]
        public AttackSuccess IsSuccess;

        [FieldOrder(1)]
        public uint MobId;

        [FieldOrder(2)]
        public uint TargetId;

        [FieldOrder(3)]
        public byte AttackType = 1; // Unknown.

        [FieldOrder(4)]
        public ushort SkillId;

        [FieldOrder(5)]
        public byte SkillLevel;

        [FieldOrder(6)]
        public ushort[] Damage;

        public MobSkillAttack(uint mobId, uint targetId, Skill skill, AttackResult attackResult)
        {
            IsSuccess = attackResult.Success;
            MobId = mobId;
            TargetId = targetId;
            SkillId = skill.SkillId;
            SkillLevel = skill.SkillLevel;
            Damage = new ushort[] { attackResult.Damage.HP, attackResult.Damage.SP, attackResult.Damage.MP };
        }
    }
}
