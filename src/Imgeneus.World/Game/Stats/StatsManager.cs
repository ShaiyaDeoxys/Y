using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Stats
{
    public class StatsManager : IStatsManager
    {
        private readonly ILogger<StatsManager> _logger;
        private readonly IDatabase _database;

        private int _ownerId;

        public StatsManager(ILogger<StatsManager> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;

#if DEBUG
            _logger.LogDebug("StatsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~StatsManager()
        {
            _logger.LogDebug("StatsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public void Init(int ownerId, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc, ushort statPoints = 0, CharacterProfession? profession = null, ushort def = 0, ushort res = 0)
        {
            _ownerId = ownerId;

            Class = profession;

            Strength = str;
            Dexterity = dex;
            Reaction = rec;
            Intelligence = intl;
            Wisdom = wis;
            Luck = luc;
            StatPoint = statPoints;
            _def = def;
            _res = res;

            _extraStr = 0;
            _extraDex = 0;
            _extraRec = 0;
            _extraIntl = 0;
            _extraLuc = 0;
            _extraWis = 0;

            ExtraDefense = 0;
            ExtraResistance = 0;

            ExtraPhysicalHittingChance = 0;
            ExtraPhysicalEvasionChance = 0;
            ExtraCriticalHittingChance = 0;
            ExtraMagicHittingChance = 0;
            ExtraMagicEvasionChance = 0;
            ExtraPhysicalAttackPower = 0;
            ExtraMagicAttackPower = 0;
        }

        #region Constants
        public CharacterProfession? Class { get; private set; }

        private ushort _str;
        public ushort Strength { get => _str; private set => _str = value; }

        private ushort _dex;
        public ushort Dexterity { get => _dex; private set { _dex = value; OnDexUpdate?.Invoke(); } }

        private ushort _rec;
        public ushort Reaction { get => _rec; private set { _rec = value; OnRecUpdate?.Invoke(); } }

        private ushort _intl;
        public ushort Intelligence { get => _intl; private set => _intl = value; }

        private ushort _luc;
        public ushort Luck { get => _luc; private set => _luc = value; }

        private ushort _wis;
        public ushort Wisdom { get => _wis; private set { _wis = value; OnWisUpdate?.Invoke(); } }

        private ushort _def;

        private ushort _res;
        #endregion

        #region Extras
        private int _extraStr;
        public int ExtraStr { get => _extraStr; set => _extraStr = value; }

        private int _extraDex;
        public int ExtraDex { get => _extraDex; set { _extraDex = value; OnDexUpdate?.Invoke(); } }

        private int _extraRec;
        public int ExtraRec { get => _extraRec; set { _extraRec = value; OnRecUpdate?.Invoke(); } }

        private int _extraIntl;
        public int ExtraInt { get => _extraIntl; set => _extraIntl = value; }

        private int _extraLuc;
        public int ExtraLuc { get => _extraLuc; set => _extraLuc = value; }

        private int _extraWis;
        public int ExtraWis { get => _extraWis; set { _extraWis = value; OnWisUpdate?.Invoke(); } }

        public int ExtraDefense { get; set; }
        public int ExtraResistance { get; set; }
        public int ExtraPhysicalHittingChance { get; set; }
        public int ExtraPhysicalEvasionChance { get; set; }
        public int ExtraCriticalHittingChance { get; set; }
        public int ExtraMagicHittingChance { get; set; }
        public int ExtraMagicEvasionChance { get; set; }
        public int ExtraPhysicalAttackPower { get; set; }
        public int ExtraMagicAttackPower { get; set; }

        #endregion

        #region Total
        public int TotalStr => Strength + ExtraStr;
        public int TotalDex => Dexterity + ExtraDex;
        public int TotalRec => Reaction + ExtraRec;
        public int TotalInt => Intelligence + ExtraInt;
        public int TotalWis => Wisdom + ExtraWis;
        public int TotalLuc => Luck + ExtraLuc;
        public int TotalDefense => _def + TotalRec + ExtraDefense;
        public int TotalResistance => _res + TotalWis + ExtraResistance;
        #endregion

        #region Hitting chances

        public double PhysicalHittingChance
        {
            get
            {
                var calculated = 1.0 * TotalDex / 2 + ExtraPhysicalHittingChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        public double PhysicalEvasionChance
        {
            get
            {
                var calculated = 1.0 * TotalDex / 2 + ExtraPhysicalEvasionChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        public double CriticalHittingChance
        {
            get
            {
                // each 5 luck is 1% of critical.
                var calculated = 0.2 * TotalLuc + ExtraCriticalHittingChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        public double MagicHittingChance
        {
            get
            {
                var calculated = 1.0 * TotalWis / 2 + ExtraMagicHittingChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        /// <summary>
        /// Possibility to escape hit.
        /// </summary>
        public double MagicEvasionChance
        {
            get
            {
                var calculated = 1.0 * TotalWis / 2 + ExtraMagicEvasionChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        #endregion

        #region Min/Max Attack & Magic attack

        public int WeaponMinAttack { get; set; }

        public int WeaponMaxAttack { get; set; }

        public int MinAttack
        {
            get
            {
                int characterAttack = 0;

                if (Class == CharacterProfession.Fighter ||
                    Class == CharacterProfession.Defender ||
                    Class == CharacterProfession.Ranger ||
                    Class == CharacterProfession.Archer)
                {
                    characterAttack = GetCharacterAttack();
                }

                return WeaponMinAttack + characterAttack + ExtraPhysicalAttackPower;
            }
        }

        /// <summary>
        /// Max physical attack.
        /// </summary>
        public int MaxAttack
        {
            get
            {
                int characterAttack = 0;

                if (Class == CharacterProfession.Fighter ||
                    Class == CharacterProfession.Defender ||
                    Class == CharacterProfession.Ranger ||
                    Class == CharacterProfession.Archer)
                {
                    characterAttack = GetCharacterAttack();
                }

                return WeaponMaxAttack + characterAttack + ExtraPhysicalAttackPower;
            }
        }

        /// <summary>
        /// Min magic attack.
        /// </summary>
        public int MinMagicAttack
        {
            get
            {
                int characterAttack = 0;

                if (Class == CharacterProfession.Mage ||
                    Class == CharacterProfession.Priest)
                {
                    characterAttack = GetCharacterAttack();
                }

                return WeaponMinAttack + characterAttack + ExtraMagicAttackPower;
            }
        }

        /// <summary>
        /// Max magic attack.
        /// </summary>
        public int MaxMagicAttack
        {
            get
            {
                int characterAttack = 0;

                if (Class == CharacterProfession.Mage ||
                    Class == CharacterProfession.Priest)
                {
                    characterAttack = GetCharacterAttack();
                }

                return WeaponMaxAttack + characterAttack + ExtraMagicAttackPower;
            }
        }

        /// <summary>
        /// Calculates character attack, based on character profession.
        /// </summary>
        private int GetCharacterAttack()
        {
            var characterAttack = 0;
            switch (Class)
            {
                case CharacterProfession.Fighter:
                case CharacterProfession.Defender:
                case CharacterProfession.Ranger:
                    characterAttack = (int)(Math.Floor(1.3 * TotalStr) + Math.Floor(0.25 * TotalDex));
                    break;

                case CharacterProfession.Mage:
                case CharacterProfession.Priest:
                    characterAttack = (int)(Math.Floor(1.3 * TotalInt) + Math.Floor(0.2 * TotalWis));
                    break;

                case CharacterProfession.Archer:
                    characterAttack = (int)(TotalStr + Math.Floor(0.3 * TotalLuc) + Math.Floor(0.2 * TotalDex));
                    break;
            }

            return characterAttack;
        }

        #endregion

        public ushort Absorption { get; set; }

        #region Stat points

        public ushort StatPoint { get; private set; }

        public async Task<bool> TrySetStats(ushort? str = null, ushort? dex = null, ushort? rec = null, ushort? intl = null, ushort? wis = null, ushort? luc = null, ushort? statPoint = null)
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                return false;

            character.Strength = str.HasValue ? str.Value : character.Strength;
            character.Dexterity = dex.HasValue ? dex.Value : character.Dexterity;
            character.Rec = rec.HasValue ? rec.Value : character.Rec;
            character.Intelligence = intl.HasValue ? intl.Value : character.Intelligence;
            character.Wisdom = wis.HasValue ? wis.Value : character.Wisdom;
            character.Luck = luc.HasValue ? luc.Value : character.Luck;
            character.StatPoint = statPoint.HasValue ? statPoint.Value : character.StatPoint;

            var count = await _database.SaveChangesAsync();

            if (count > 0)
            {
                Strength = character.Strength;
                Dexterity = character.Dexterity;
                Reaction = character.Rec;
                Intelligence = character.Intelligence;
                Wisdom = character.Wisdom;
                Luck = character.Luck;
                StatPoint = character.StatPoint;

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Events

        public event Action OnRecUpdate;
        public event Action OnDexUpdate;
        public event Action OnWisUpdate;

        public event Action OnAdditionalStatsUpdate;

        public void RaiseAdditionalStatsUpdate()
        {
            OnAdditionalStatsUpdate?.Invoke();
        }

        public event Action OnResetStats;
        public void RaiseResetStats()
        {
            OnResetStats?.Invoke();
        }

        #endregion
    }
}
