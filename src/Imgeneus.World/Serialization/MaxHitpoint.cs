using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization
{
    public class MaxHitpoint : BaseSerializable
    {
        [FieldOrder(0)]
        public uint CharacterId;

        [FieldOrder(1)]
        public HitpointType HitpointType;

        [FieldOrder(2)]
        public int Value;

        public MaxHitpoint(uint characterId, HitpointType hitpointType, int value)
        {
            CharacterId = characterId;
            HitpointType = hitpointType;
            Value = value;
        }
    }
}
