using BinarySerialization;
using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.Network.Serialization;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Serialization.EP_8_V1
{
    public class CharacterShape : BaseSerializable
    {
        [FieldOrder(0)]
        public bool IsDead { get; }

        [FieldOrder(1)]
        public Motion Motion { get; }

        [FieldOrder(2)]
        public CountryType Country { get; }

        [FieldOrder(3)]
        public Race Race { get; }

        [FieldOrder(4)]
        public byte Hair { get; }

        [FieldOrder(5)]
        public byte Face { get; }

        [FieldOrder(6)]
        public byte Height { get; }

        [FieldOrder(7)]
        public CharacterProfession Class { get; }

        [FieldOrder(8)]
        public Gender Gender { get; }

        [FieldOrder(9)]
        public byte PartyDefinition { get; }

        [FieldOrder(10)]
        public Mode Mode { get; }

        [FieldOrder(11)]
        public uint Kills { get; }

        [FieldOrder(12)]
        public EquipmentItem[] EquipmentItems { get; } = new EquipmentItem[17];

        [FieldOrder(13)]
        public bool[] EquipmentItemHasColor { get; } = new bool[17];

        [FieldOrder(14)]
        public int UnknownInt { get; }

        [FieldOrder(15)]
        public DyeColorSerialized[] EquipmentItemColor { get; } = new DyeColorSerialized[17];

        [FieldOrder(16)]
        public byte[] UnknownBytes2 { get; } = new byte[451];

        [FieldOrder(17), FieldLength(21)]
        public string Name;

        [FieldOrder(18)]
        public byte[] UnknownBytes3 = new byte[24];

        [FieldOrder(19), FieldLength(25)]
        public string GuildName;

        public CharacterShape(Character character)
        {
            IsDead = character.HealthManager.IsDead;
            Motion = character.MovementManager.Motion;
            Country = character.CountryProvider.Country;
            Race = character.AdditionalInfoManager.Race;
            Hair = character.AdditionalInfoManager.Hair;
            Face = character.AdditionalInfoManager.Face;
            Height = character.AdditionalInfoManager.Height;
            Class = character.AdditionalInfoManager.Class;
            Gender = character.AdditionalInfoManager.Gender;
            Mode = character.AdditionalInfoManager.Grow;
            Kills = character.KillsManager.Kills;
            Name = character.AdditionalInfoManager.FakeName is null ? character.AdditionalInfoManager.Name : character.AdditionalInfoManager.FakeName;
            GuildName = character.AdditionalInfoManager.FakeGuildName is null ? character.GuildManager.GuildName : character.AdditionalInfoManager.FakeGuildName;

            for (byte i = 0; i < 17; i++)
            {
                character.InventoryManager.InventoryItems.TryGetValue((0, i), out var item);
                EquipmentItems[i] = new EquipmentItem(item);

                if (item != null)
                {
                    EquipmentItemHasColor[i] = item.DyeColor.IsEnabled;
                    if (item.DyeColor.IsEnabled)
                        EquipmentItemColor[i] = new DyeColorSerialized(item.DyeColor.Saturation, item.DyeColor.R, item.DyeColor.G, item.DyeColor.B);
                    else
                        EquipmentItemColor[i] = new DyeColorSerialized();
                }
                else
                    EquipmentItemColor[i] = new DyeColorSerialized();
            }

            if (character.PartyManager.HasParty)
            {
                if (character.PartyManager.IsPartyLead)
                {
                    PartyDefinition = 2;
                }
                else
                {
                    PartyDefinition = 1;
                }
            }
            else
            {
                PartyDefinition = 0;
            }
        }
    }
}
