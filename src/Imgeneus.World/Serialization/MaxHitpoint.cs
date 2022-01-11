using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class MaxHitpoint : BaseSerializable
    {
        [FieldOrder(0)]
        public int CharacterId;

        [FieldOrder(1)]
        public HitpointType HitpointType;

        [FieldOrder(2)]
        public int Value;

        public MaxHitpoint(int characterId, HitpointType hitpointType, int value)
        {
            CharacterId = characterId;
            HitpointType = hitpointType;
            Value = value;
        }
    }
}
