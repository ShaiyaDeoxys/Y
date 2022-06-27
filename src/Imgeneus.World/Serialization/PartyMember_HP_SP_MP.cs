using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class PartyMember_HP_SP_MP : BaseSerializable
    {
        [FieldOrder(0)]
        public uint CharacterId { get; }

        [FieldOrder(1)]
        public int CurrentHP { get; }

        [FieldOrder(2)]
        public int CurrentSP { get; }

        [FieldOrder(3)]
        public int CurrentMP { get; }

        public PartyMember_HP_SP_MP(Character partyMember)
        {
            CharacterId = partyMember.Id;
            CurrentHP = partyMember.HealthManager.CurrentHP;
            CurrentSP = partyMember.HealthManager.CurrentSP;
            CurrentMP = partyMember.HealthManager.CurrentMP;
        }
    }
}
