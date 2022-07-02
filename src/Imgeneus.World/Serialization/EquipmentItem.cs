using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Inventory;

namespace Imgeneus.World.Serialization
{
    public class EquipmentItem : BaseSerializable
    {
        [FieldOrder(0)]
        public byte Type { get; }

        [FieldOrder(1)]
        public byte TypeId { get; }

        [FieldOrder(2)]
        public byte EnhancementLevel { get; }

        public EquipmentItem(Item item)
        {
            if (item != null)
            {
                Type = item.Type;
                TypeId = item.TypeId;
                EnhancementLevel = item.EnchantmentLevel;
            }
        }
    }
}
