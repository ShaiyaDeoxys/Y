using BinarySerialization;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Inventory;

namespace Imgeneus.World.Serialization.SHAIYA_US
{
    public class CharacterEquipmentChange : BaseSerializable
    {
        [FieldOrder(0)]
        public uint CharacterId;

        [FieldOrder(1)]
        public byte Slot;

        [FieldOrder(2)]
        public byte Type;

        [FieldOrder(3)]
        public byte TypeId;

        [FieldOrder(4)]
        public byte EnchantLevel;

        [FieldOrder(5)]
        public bool HasColor;

        [FieldOrder(6)]
        public DyeColorSerialized DyeColor;

        public CharacterEquipmentChange(uint characterId, byte slot, Item item)
        {
            CharacterId = characterId;
            Slot = slot;

            if (item != null)
            {
                Type = item.Type;
                TypeId = item.TypeId;
                EnchantLevel = item.EnchantmentLevel;
                HasColor = item.DyeColor.IsEnabled;
                if (HasColor)
                    DyeColor = new DyeColorSerialized(item.DyeColor.Alpha, item.DyeColor.R, item.DyeColor.G, item.DyeColor.B, item.DyeColor.Saturation);
                else
                    DyeColor = new DyeColorSerialized();
            }
            else
                DyeColor = new DyeColorSerialized();
        }
    }
}
