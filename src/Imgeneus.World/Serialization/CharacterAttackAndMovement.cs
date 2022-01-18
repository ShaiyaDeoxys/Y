using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Speed;

namespace Imgeneus.World.Serialization
{
    public class CharacterAttackAndMovement : BaseSerializable
    {
        [FieldOrder(0)]
        public int CharacterId;

        [FieldOrder(1)]
        public AttackSpeed AttackSpeed { get; }

        [FieldOrder(2)]
        public MoveSpeed MoveSpeed { get; }

        public CharacterAttackAndMovement(int characterId, AttackSpeed attack, MoveSpeed move)
        {
            CharacterId = characterId;
            AttackSpeed = attack;
            MoveSpeed = move;
        }
    }
}
