using Imgeneus.Database.Entities;

namespace Imgeneus.World.Game.Player.Config
{
    public class CreationConfiguration
    {
        public Fraction Country { get; set; }

        public CharacterProfession Job { get; set; }

        public ushort MapId { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public StartItem[] StartItems { get; set; }
    }
}