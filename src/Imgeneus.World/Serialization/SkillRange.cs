using BinarySerialization;
using Imgeneus.Game.Skills;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Attack;

namespace Imgeneus.World.Serialization
{
    public class SkillRange : BaseSerializable
    {
        [FieldOrder(0)]
        public AttackSuccess IsSuccess { get; }

        [FieldOrder(1)]
        public uint CharacterId { get; }

        [FieldOrder(2)]
        public uint TargetId { get; }

        [FieldOrder(3)]
        public ushort SkillId { get; }

        [FieldOrder(4)]
        public byte SkillLevel { get; }

        [FieldOrder(5)]
        public ushort[] Damage = new ushort[3];

        [FieldOrder(6)]
        public bool KeepActivated { get; }

        public SkillRange(uint characterId, uint targetId, ushort skillId, byte skillLevel, AttackResult attackResult, bool keepActivated)
        {
            IsSuccess = attackResult.Success;
            CharacterId = characterId;
            TargetId = targetId;
            SkillId = skillId;
            SkillLevel = skillLevel;
            Damage = new ushort[] { attackResult.Damage.HP, attackResult.Damage.SP, attackResult.Damage.MP };
            KeepActivated = keepActivated;
        }
    }
}
