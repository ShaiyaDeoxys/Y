using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class CharacterHitpoints : BaseSerializable
    {
        [FieldOrder(0)]
        public int HP { get; }

        [FieldOrder(1)]
        public int MP { get; }

        [FieldOrder(2)]
        public int SP { get; }

        public CharacterHitpoints(Character character)
        {
            HP = character.HealthManager.CurrentHP;
            MP = character.HealthManager.CurrentMP;
            SP = character.HealthManager.CurrentSP;
        }
    }
}
