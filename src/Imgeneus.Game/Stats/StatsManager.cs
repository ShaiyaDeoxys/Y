using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Stats
{
    public class StatsManager : IStatsManager
    {
        private readonly ILogger<StatsManager> _logger;
        private readonly IDatabase _database;
        private readonly ILevelProvider _levelProvider;
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private readonly ICharacterConfiguration _characterConfig;
        private uint _ownerId;

        public StatsManager(ILogger<StatsManager> logger, IDatabase database, ILevelProvider levelProvider, IAdditionalInfoManager additionalInfoManager, ICharacterConfiguration charactedConfig)
        {
            _logger = logger;
            _database = database;
            _levelProvider = levelProvider;
            _additionalInfoManager = additionalInfoManager;
            _characterConfig = charactedConfig;

            _levelProvider.OnLevelUp += OnLevelUp;

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

        #region Init & Clear

        public void Init(uint ownerId, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc, ushort statPoints = 0, CharacterProfession? profession = null, ushort def = 0, ushort res = 0, byte autoStr = 0, byte autoDex = 0, byte autoRec = 0, byte autoInt = 0, byte autoWis = 0, byte autoLuc = 0)
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

            AutoStr = autoStr;
            AutoDex = autoDex;
            AutoRec = autoRec;
            AutoInt = autoInt;
            AutoWis = autoWis;
            AutoLuc = autoLuc;

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

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);

            character.Strength = Strength;
            character.Dexterity = Dexterity;
            character.Rec = Reaction;
            character.Intelligence = Intelligence;
            character.Wisdom = Wisdom;
            character.Luck = Luck;
            character.StatPoint = StatPoint;

            await _database.SaveChangesAsync();
        }

        public void Dispose()
        {
            _levelProvider.OnLevelUp -= OnLevelUp;
        }

        #endregion

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
        public int ExtraShootingHittingChance { get; set; }
        public int ExtraShootingEvasionChance { get; set; }
        public int ExtraCriticalHittingChance { get; set; }
        public int ExtraMagicHittingChance { get; set; }
        public int ExtraMagicEvasionChance { get; set; }
        public int ExtraPhysicalAttackPower { get; set; }
        public int ExtraMagicAttackPower { get; set; }

        public byte ShieldDefencePassiveSkillModificator { get; set; }

        #endregion

        #region Total
        public int TotalStr
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseStrBySacrificing)
                {
                    increaseBySacrificing += SacrificedDex;
                    increaseBySacrificing += SacrificedRec;
                    increaseBySacrificing += SacrificedInt;
                    increaseBySacrificing += SacrificedWis;
                    increaseBySacrificing += SacrificedLuc;
                }

                var total = Strength + ExtraStr - SacrificedStr + increaseBySacrificing;

                return total > 0 ? total : 0;
            }
        }

        public int TotalDex
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseDexBySacrificing)
                {
                    increaseBySacrificing += SacrificedStr;
                    increaseBySacrificing += SacrificedRec;
                    increaseBySacrificing += SacrificedInt;
                    increaseBySacrificing += SacrificedWis;
                    increaseBySacrificing += SacrificedLuc;
                }

                var total = Dexterity + ExtraDex - SacrificedDex + increaseBySacrificing;
                return total > 0 ? total : 0;
            }
        }
        public int TotalRec
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseRecBySacrificing)
                {
                    increaseBySacrificing += SacrificedStr;
                    increaseBySacrificing += SacrificedDex;
                    increaseBySacrificing += SacrificedInt;
                    increaseBySacrificing += SacrificedWis;
                    increaseBySacrificing += SacrificedLuc;
                }

                var total = Reaction + ExtraRec - SacrificedRec + increaseBySacrificing;
                return total > 0 ? total : 0;
            }
        }
        public int TotalInt
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseIntBySacrificing)
                {
                    increaseBySacrificing += SacrificedStr;
                    increaseBySacrificing += SacrificedDex;
                    increaseBySacrificing += SacrificedRec;
                    increaseBySacrificing += SacrificedWis;
                    increaseBySacrificing += SacrificedLuc;
                }

                var total = Intelligence + ExtraInt - SacrificedInt + increaseBySacrificing;
                return total > 0 ? total : 0;
            }
        }
        public int TotalWis
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseWisBySacrificing)
                {
                    increaseBySacrificing += SacrificedStr;
                    increaseBySacrificing += SacrificedDex;
                    increaseBySacrificing += SacrificedRec;
                    increaseBySacrificing += SacrificedInt;
                    increaseBySacrificing += SacrificedLuc;
                }

                var total = Wisdom + ExtraWis - SacrificedWis + increaseBySacrificing;
                return total > 0 ? total : 0;
            }
        }

        public int TotalLuc
        {
            get
            {
                var increaseBySacrificing = 0;
                if (IncreaseLucBySacrificing)
                {
                    increaseBySacrificing += SacrificedStr;
                    increaseBySacrificing += SacrificedDex;
                    increaseBySacrificing += SacrificedRec;
                    increaseBySacrificing += SacrificedInt;
                    increaseBySacrificing += SacrificedWis;
                }

                var total = Luck + ExtraLuc - SacrificedLuc + increaseBySacrificing;
                return total > 0 ? total : 0;
            }
        }

        public int TotalDefense
        {
            get
            {
                var total = _def + Reaction + ExtraRec + ExtraDefense + ShieldDefencePassiveSkillModificator;
                if (total > 0 && DefencePersent > 0)
                    total = total + total * DefencePersent / 100;

                return total > 0 ? total : 0;
            }
        }

        public int TotalResistance
        {
            get
            {
                var total = _res + Wisdom + ExtraWis + ExtraResistance;
                if (total > 0 && ResistancePersent > 0)
                    total = total + total * ResistancePersent / 100;

                return total > 0 ? total : 0;
            }
        }

        #endregion

        #region Auto stats

        public byte AutoStr { get; private set; }

        public byte AutoDex { get; private set; }

        public byte AutoRec { get; private set; }

        public byte AutoInt { get; private set; }

        public byte AutoLuc { get; private set; }

        public byte AutoWis { get; private set; }

        public async Task<bool> TrySetAutoStats(byte str, byte dex, byte rec, byte intl, byte wis, byte luc)
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                return false;

            character.AutoStr = str;
            character.AutoDex = dex;
            character.AutoRec = rec;
            character.AutoInt = intl;
            character.AutoWis = wis;
            character.AutoLuc = luc;

            var count = await _database.SaveChangesAsync();

            if (count > 0)
            {
                AutoStr = character.AutoStr;
                AutoDex = character.AutoDex;
                AutoRec = character.AutoRec;
                AutoInt = character.AutoInt;
                AutoWis = character.AutoWis;
                AutoLuc = character.AutoLuc;

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Sacrifice

        public ushort SacrificedStrPercent { get; set; }

        public int SacrificedStr => SacrificedStrPercent > 0 ? (Strength + ExtraStr) / SacrificedStrPercent : 0;

        public ushort SacrificedDexPercent { get; set; }

        public int SacrificedDex => SacrificedDexPercent > 0 ? (Dexterity + ExtraDex) / SacrificedDexPercent : 0;

        public ushort SacrificedRecPercent { get; set; }

        public int SacrificedRec => SacrificedRecPercent > 0 ? (Reaction + ExtraRec) / SacrificedRecPercent : 0;

        public ushort SacrificedIntPercent { get; set; }

        public int SacrificedInt => SacrificedIntPercent > 0 ? (Intelligence + ExtraInt) / SacrificedIntPercent : 0;

        public ushort SacrificedWisPercent { get; set; }

        public int SacrificedWis => SacrificedWisPercent > 0 ? (Wisdom + ExtraWis) / SacrificedWisPercent : 0;

        public ushort SacrificedLucPercent { get; set; }

        public int SacrificedLuc => SacrificedLucPercent > 0 ? (Luck + ExtraLuc) / SacrificedLucPercent : 0;

        public bool IncreaseStrBySacrificing { get; set; }

        public bool IncreaseDexBySacrificing { get; set; }

        public bool IncreaseRecBySacrificing { get; set; }

        public bool IncreaseIntBySacrificing { get; set; }

        public bool IncreaseWisBySacrificing { get; set; }

        public bool IncreaseLucBySacrificing { get; set; }

        #endregion

        #region % increase

        public int DefencePersent { get; set; }

        public int ResistancePersent { get; set; }

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

        public double ShootingHittingChance
        {
            get
            {
                var calculated = 1.0 * TotalDex / 2 + ExtraShootingHittingChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        public double ShootingEvasionChance
        {
            get
            {
                var calculated = 1.0 * TotalDex / 2 + ExtraShootingEvasionChance;
                return calculated > 0 ? calculated : 1;
            }
        }

        public byte ConstShootingEvasionChance { get; set; }

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

        public byte WeaponType { get; set; }

        public Dictionary<byte, byte> WeaponAttackPassiveSkillModificator { get; init; } = new();

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

                WeaponAttackPassiveSkillModificator.TryGetValue(WeaponType, out var passivePowerUp);

                return WeaponMinAttack + characterAttack + ExtraPhysicalAttackPower + passivePowerUp;
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

                WeaponAttackPassiveSkillModificator.TryGetValue(WeaponType, out var passivePowerUp);

                return WeaponMaxAttack + characterAttack + ExtraPhysicalAttackPower + passivePowerUp;
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

                WeaponAttackPassiveSkillModificator.TryGetValue(WeaponType, out var passivePowerUp);

                return WeaponMinAttack + characterAttack + ExtraMagicAttackPower + passivePowerUp;
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

                WeaponAttackPassiveSkillModificator.TryGetValue(WeaponType, out var passivePowerUp);

                return WeaponMaxAttack + characterAttack + ExtraMagicAttackPower + passivePowerUp;
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

        public bool TrySetStats(ushort? str = null, ushort? dex = null, ushort? rec = null, ushort? intl = null, ushort? wis = null, ushort? luc = null, ushort? statPoint = null)
        {
            Strength = str ?? Strength;
            Dexterity = dex ?? Dexterity;
            Reaction = rec ?? Reaction;
            Intelligence = intl ?? Intelligence;
            Wisdom = wis ?? Wisdom;
            Luck = luc ?? Luck;
            StatPoint = statPoint ?? StatPoint;

            return true;
        }

        private void OnLevelUp(uint senderId, ushort level, ushort oldLevel)
        {
            var levelDifference = level - oldLevel;

            if (levelDifference > 0)
                IncreasePrimaryStat((ushort)levelDifference);
            else
                DecreasePrimaryStat((ushort)Math.Abs(levelDifference));

            var levelStats = _characterConfig.GetLevelStatSkillPoints(_additionalInfoManager.Grow);
            TrySetStats(statPoint: (ushort)(StatPoint + levelStats.StatPoint));
        }

        #endregion

        #region Primary stats

        /// <summary>
        /// Increases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        public void IncreasePrimaryStat(ushort amount = 1)
        {
            var primaryAttribute = _additionalInfoManager.GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    TrySetStats(str: (ushort)(Strength + amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    TrySetStats(dex: (ushort)(Dexterity + amount));
                    break;

                case CharacterStatEnum.Reaction:
                    TrySetStats(rec: (ushort)(Reaction + amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    TrySetStats(intl: (ushort)(Intelligence + amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    TrySetStats(wis: (ushort)(Wisdom + amount));
                    break;

                case CharacterStatEnum.Luck:
                    TrySetStats(luc: (ushort)(Luck + amount));
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
            var primaryAttribute = _additionalInfoManager.GetPrimaryStat();

            switch (primaryAttribute)
            {
                case CharacterStatEnum.Strength:
                    TrySetStats(str: (ushort)(Strength - amount));
                    break;

                case CharacterStatEnum.Dexterity:
                    TrySetStats(dex: (ushort)(Dexterity - amount));
                    break;

                case CharacterStatEnum.Reaction:
                    TrySetStats(rec: (ushort)(Reaction - amount));
                    break;

                case CharacterStatEnum.Intelligence:
                    TrySetStats(intl: (ushort)(Intelligence - amount));
                    break;

                case CharacterStatEnum.Wisdom:
                    TrySetStats(wis: (ushort)(Wisdom - amount));
                    break;

                case CharacterStatEnum.Luck:
                    TrySetStats(luc: (ushort)(Luck - amount));
                    break;

                default:
                    break;
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
