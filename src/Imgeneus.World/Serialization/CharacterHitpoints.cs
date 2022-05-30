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

        public CharacterHitpoints(int hp, int mp, int sp)
        {
            HP = hp;
            MP = mp;
            SP = sp;
        }
    }
}
