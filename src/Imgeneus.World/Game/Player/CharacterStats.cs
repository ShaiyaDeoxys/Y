using Imgeneus.Database.Entities;
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
        public Race Race { get; set; }
        public CharacterProfession Class { get; set; }
        public byte Hair { get; set; }
        public byte Face { get; set; }
        public byte Height { get; set; }
        public Gender Gender { get; set; }
        public uint Exp { get; private set; }
        public bool IsAdmin { get; set; }
        public bool IsRename { get; set; }

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
        public CharacterStatEnum GetPrimaryStat()
        {
            var defaultStat = _characterConfig.DefaultStats.First(s => s.Job == Class);

            switch (defaultStat.MainStat)
            {
                case 0:
                    return CharacterStatEnum.Strength;

                case 1:
                    return CharacterStatEnum.Dexterity;

                case 2:
                    return CharacterStatEnum.Reaction;

                case 3:
                    return CharacterStatEnum.Intelligence;

                case 4:
                    return CharacterStatEnum.Wisdom;

                case 5:
                    return CharacterStatEnum.Luck;

                default:
                    throw new NotSupportedException();
            }
        }

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

        /// <summary>
        /// Increases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        public void IncreasePrimaryStat(ushort amount = 1)
        {
            var primaryAttribute = GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    StatsManager.TrySetStats(str: (ushort)(StatsManager.Strength + amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    StatsManager.TrySetStats(dex: (ushort)(StatsManager.Dexterity + amount));
                    break;

                case CharacterStatEnum.Reaction:
                    StatsManager.TrySetStats(rec: (ushort)(StatsManager.Reaction + amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    StatsManager.TrySetStats(intl: (ushort)(StatsManager.Intelligence + amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    StatsManager.TrySetStats(wis: (ushort)(StatsManager.Wisdom + amount));
                    break;

                case CharacterStatEnum.Luck:
                    StatsManager.TrySetStats(luc: (ushort)(StatsManager.Luck + amount));
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Decreases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        public void DecreasePrimaryStat(ushort amount = 1)
        {
            var primaryAttribute = GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    StatsManager.TrySetStats(str: (ushort)(StatsManager.Strength - amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    StatsManager.TrySetStats(dex: (ushort)(StatsManager.Dexterity - amount));
                    break;

                case CharacterStatEnum.Reaction:
                    StatsManager.TrySetStats(rec: (ushort)(StatsManager.Reaction - amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    StatsManager.TrySetStats(intl: (ushort)(StatsManager.Intelligence - amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    StatsManager.TrySetStats(wis: (ushort)(StatsManager.Wisdom - amount));
                    break;

                case CharacterStatEnum.Luck:
                    StatsManager.TrySetStats(luc: (ushort)(StatsManager.Luck - amount));
                    break;

                default:
                    break;
            }
        }
 
        #endregion

        #region Reset stats

        public void ResetStats()
        {
            var defaultStat = _characterConfig.DefaultStats.First(s => s.Job == Class);
            var statPerLevel = _characterConfig.GetLevelStatSkillPoints(LevelingManager.Grow).StatPoint;

            StatsManager.TrySetStats(defaultStat.Str,
                                     defaultStat.Dex,
                                     defaultStat.Rec,
                                     defaultStat.Int,
                                     defaultStat.Luc,
                                     (ushort)((LevelProvider.Level - 1) * statPerLevel)); // Level - 1, because we are starting with 1 level.

            IncreasePrimaryStat((ushort)(LevelProvider.Level - 1));

            _taskQueue.Enqueue(ActionType.UPDATE_STATS, Id, StatsManager.Strength, StatsManager.Dexterity, StatsManager.Reaction, StatsManager.Intelligence, StatsManager.Wisdom, StatsManager.Luck, StatsManager.StatPoint);
            _packetsHelper.SendResetStats(Client, this);
            //SendAdditionalStats();
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
