using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Skills;

namespace Imgeneus.World.Serialization
{
    public class SkillRange : BaseSerializable
    {
        [FieldOrder(0)]
        public AttackSuccess IsSuccess { get; }

        [FieldOrder(1)]
        public int CharacterId { get; }

        [FieldOrder(2)]
        public int TargetId { get; }

        [FieldOrder(3)]
        public ushort SkillId { get; }

        [FieldOrder(4)]
        public byte SkillLevel { get; }

        [FieldOrder(5)]
        public ushort[] Damage = new ushort[3];

        public SkillRange(int characterId, int targetId, Skill skill, AttackResult attackResult)
        {
            IsSuccess = attackResult.Success;
            CharacterId = characterId;
            TargetId = targetId;
            SkillId = skill.SkillId;
            SkillLevel = skill.SkillLevel;
            Damage = new ushort[] { attackResult.Damage.HP, attackResult.Damage.SP, attackResult.Damage.MP };
        }
    }
}
