using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Stats;
using System;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        #region Character info

        public string Name { get; set; } = "";
        public ushort MapId { get; private set; }
       
        public uint Exp { get; private set; }
        public bool IsAdmin { get; set; }

        /// <summary>
        /// Account points, used for item mall or online shop purchases.
        /// </summary>
        public uint Points { get; private set; }

        private byte[] _nameAsByteArray;
        public byte[] NameAsByteArray
        {
            get
            {
                if (_nameAsByteArray is null)
                {
                    _nameAsByteArray = new byte[21];

                    var chars = Name.ToCharArray(0, Name.Length);
                    for (var i = 0; i < chars.Length; i++)
                    {
                        _nameAsByteArray[i] = (byte)chars[i];
                    }
                }
                return _nameAsByteArray;
            }
        }

        #endregion

        #region Max HP & SP & MP

        /// <summary>
        /// Gets the character's primary stat
        /// </summary>
        public CharacterAttributeEnum GetAttributeByStat(CharacterStatEnum stat)
        {
            switch (stat)
            {
                case CharacterStatEnum.Strength:
                    return CharacterAttributeEnum.Strength;

                case CharacterStatEnum.Dexterity:
                    return CharacterAttributeEnum.Dexterity;

                case CharacterStatEnum.Reaction:
                    return CharacterAttributeEnum.Reaction;

                case CharacterStatEnum.Intelligence:
                    return CharacterAttributeEnum.Intelligence;

                case CharacterStatEnum.Wisdom:
                    return CharacterAttributeEnum.Wisdom;

                case CharacterStatEnum.Luck:
                    return CharacterAttributeEnum.Luck;

                default:
                    throw new NotSupportedException();
            }
        }
 
        #endregion

        #region Attributes

        /// <summary>
        /// Gets a character's attribute.
        /// </summary>
        public uint GetAttributeValue(CharacterAttributeEnum attribute)
        {
            switch (attribute)
            {
                case CharacterAttributeEnum.Grow:
                    return (uint)LevelingManager.Grow;

                case CharacterAttributeEnum.Level:
                    return LevelProvider.Level;

                case CharacterAttributeEnum.Money:
                    return InventoryManager.Gold;

                case CharacterAttributeEnum.StatPoint:
                    return StatsManager.StatPoint;

                case CharacterAttributeEnum.SkillPoint:
                    return SkillsManager.SkillPoints;

                case CharacterAttributeEnum.Strength:
                    return StatsManager.Strength;

                case CharacterAttributeEnum.Dexterity:
                    return StatsManager.Dexterity;

                case CharacterAttributeEnum.Reaction:
                    return StatsManager.Reaction;

                case CharacterAttributeEnum.Intelligence:
                    return StatsManager.Intelligence;

                case CharacterAttributeEnum.Luck:
                    return StatsManager.Luck;

                case CharacterAttributeEnum.Wisdom:
                    return StatsManager.Wisdom;

                // TODO: Investigate what these attributes represent
                case CharacterAttributeEnum.Hg:
                case CharacterAttributeEnum.Vg:
                case CharacterAttributeEnum.Cg:
                case CharacterAttributeEnum.Og:
                case CharacterAttributeEnum.Ig:
                    return 0;

                case CharacterAttributeEnum.Exp:
                    return Exp;

                case CharacterAttributeEnum.Kills:
                    return KillsManager.Kills;

                case CharacterAttributeEnum.Deaths:
                    return KillsManager.Deaths;

                default:
                    return 0;
            }
        }

        #endregion

        #region Stat and Skill Points

        /// <summary>
        /// Increases the player's stat points by a certain amount
        /// </summary>
        /// <param name="amount"></param>
        //public void IncreaseStatPoint(ushort amount) => StatsManager.TrySetStatPoint((ushort)(StatsManager.StatPoint + amount));

        /// <summary>
        /// Increases the player's skill points by a certain amount
        /// </summary>
        /// <param name="amount"></param>
        //public void IncreaseSkillPoint(ushort amount) => SetSkillPoint(SkillPoint += amount);

        #endregion

        #region Account Points

        /// <summary>
        /// Attempts to set the player's account points.
        /// </summary>
        /// <param name="points">Points to set.</param>
        public void SetPoints(uint points)
        {
            Points = points;

            _taskQueue.Enqueue(ActionType.SAVE_ACCOUNT_POINTS, Client.UserId, Points);
            SendAccountPoints();
        }

        #endregion
    }
}
