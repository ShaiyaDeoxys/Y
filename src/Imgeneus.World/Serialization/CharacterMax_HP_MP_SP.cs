using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class CharacterMax_HP_MP_SP : BaseSerializable
    {
        public uint CharacterId { get; }
        public int MaxHP { get; }
        public int MaxMP { get; }
        public int MaxSP { get; }

        public CharacterMax_HP_MP_SP(Character character)
        {
            CharacterId = character.Id;
            MaxHP = character.HealthManager.MaxHP;
            MaxMP = character.HealthManager.MaxSP;
            MaxSP = character.HealthManager.MaxSP;
        }
    }
}