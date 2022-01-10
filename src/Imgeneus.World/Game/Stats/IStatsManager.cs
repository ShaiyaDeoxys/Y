namespace Imgeneus.World.Game.Stats
{
    public interface IStatsManager
    {
        /// <summary>
        /// Yellow strength stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraStr { get; set; }

        /// <summary>
        /// Yellow dexterity stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraDex { get; set; }

        /// <summary>
        /// Yellow rec stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraRec { get; set; }

        /// <summary>
        /// Yellow intelligence stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraInt { get; set; }

        /// <summary>
        /// Yellow luck stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraLuc { get; set; }

        /// <summary>
        /// Yellow wisdom stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        public int ExtraWis { get; set; }

        /// <summary>
        /// Physical defense from equipment and buffs.
        /// </summary>
        public int ExtraDefense { get; set; }

        /// <summary>
        /// Magical resistance from equipment and buffs.
        /// </summary>
        public int ExtraResistance { get; set; }

        /// <summary>
        /// Health points, that are provided by equipment and buffs.
        /// </summary>
        public int ExtraHP { get; set; }

        /// <summary>
        /// Stamina points, that are provided by equipment and buffs.
        /// </summary>
        public int ExtraSP { get; set; }

        /// <summary>
        /// Mana points, that are provided by equipment and buffs.
        /// </summary>
        public int ExtraMP { get; set; }


        /// <summary>
        /// Absorbs damage regardless of REC value.
        /// </summary>
        public ushort Absorption { get; set; }

    }
}
