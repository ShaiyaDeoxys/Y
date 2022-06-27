using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Stats
{
    public interface IStatsManager : ISessionedService, IDisposable
    {
        /// <summary>
        /// Inits constant stats.
        /// </summary>
        void Init(uint ownerId, ushort str, ushort dex, ushort rec, ushort intl, ushort wis, ushort luc, ushort statPoints = 0, CharacterProfession? profession = null, ushort def = 0, ushort res = 0, byte autoStr = 0, byte autoDex = 0, byte autoRec = 0, byte autoInt = 0, byte autoWis = 0, byte autoLuc = 0);

        /// <summary>
        /// Str value, needed for attack calculation.
        /// </summary>
        int TotalStr { get; }

        /// <summary>
        /// Dex value, needed for damage calculation.
        /// </summary>
        int TotalDex { get; }

        /// <summary>
        /// Rec value, needed for HP calculation.
        /// </summary>
        int TotalRec { get; }

        /// <summary>
        /// Int value, needed for damage calculation.
        /// </summary>
        int TotalInt { get; }

        /// <summary>
        /// Wis value, needed for damage calculation.
        /// </summary>
        int TotalWis { get; }

        /// <summary>
        /// Luck value, needed for critical damage calculation.
        /// </summary>
        int TotalLuc { get; }

        /// <summary>
        /// Physical defense.
        /// </summary>
        int TotalDefense { get; }

        /// <summary>
        /// Magic resistance.
        /// </summary>
        int TotalResistance { get; }

        /// <summary>
        /// Constant str.
        /// </summary>
        ushort Strength { get; }

        /// <summary>
        /// Constant dex.
        /// </summary>
        ushort Dexterity { get; }

        /// <summary>
        /// Constant rec.
        /// </summary>
        ushort Reaction { get; }

        /// <summary>
        /// Constant int.
        /// </summary>
        ushort Intelligence { get; }

        /// <summary>
        /// Constant luc.
        /// </summary>
        ushort Luck { get; }

        /// <summary>
        /// Constant wis.
        /// </summary>
        ushort Wisdom { get; }

        /// <summary>
        /// Yellow strength stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraStr { get; set; }

        /// <summary>
        /// Yellow dexterity stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraDex { get; set; }

        /// <summary>
        /// Yellow rec stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraRec { get; set; }

        /// <summary>
        /// Yellow intelligence stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraInt { get; set; }

        /// <summary>
        /// Yellow luck stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraLuc { get; set; }

        /// <summary>
        /// Yellow wisdom stat, that is calculated based on worn items, orange stats and active buffs.
        /// </summary>
        int ExtraWis { get; set; }

        /// <summary>
        /// Physical defense from equipment and buffs.
        /// </summary>
        int ExtraDefense { get; set; }

        /// <summary>
        /// Magical resistance from equipment and buffs.
        /// </summary>
        int ExtraResistance { get; set; }

        /// <summary>
        /// Gets or sets strength stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoStr { get; }

        /// <summary>
        /// Gets or sets dexterity stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoDex { get; }

        /// <summary>
        /// Gets or sets rec stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoRec { get; }

        /// <summary>
        /// Gets or sets intelligence stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoInt { get; }

        /// <summary>
        /// Gets or sets luck stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoLuc { get; }

        /// <summary>
        /// Gets or sets wisdom stat, that is set automatically, when player selects auto settings.
        /// </summary>
        byte AutoWis { get; }

        /// <summary>
        /// Str %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedStrPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed str.
        /// </summary>
        int SacrificedStr { get; }

        /// <summary>
        /// Dex %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedDexPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed dex.
        /// </summary>
        int SacrificedDex { get; }

        /// <summary>
        /// Rec %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedRecPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed rec.
        /// </summary>
        int SacrificedRec { get; }

        /// <summary>
        /// Int %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedIntPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed int.
        /// </summary>
        int SacrificedInt { get; }

        /// <summary>
        /// Wis %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedWisPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed wis.
        /// </summary>
        int SacrificedWis { get; }

        /// <summary>
        /// Luc %, that is sacrificed for another stat.
        /// </summary>
        ushort SacrificedLucPercent { get; set; }

        /// <summary>
        /// Rounded sacrificed luc.
        /// </summary>
        int SacrificedLuc { get; }

        /// <summary>
        /// Str is increase by sacrificed stats.
        /// </summary>
        bool IncreaseStrBySacrificing { get; set; }

        /// <summary>
        /// Dex is increase by sacrificed stats.
        /// </summary>
        bool IncreaseDexBySacrificing { get; set; }

        /// <summary>
        /// Rec is increase by sacrificed stats.
        /// </summary>
        bool IncreaseRecBySacrificing { get; set; }

        /// <summary>
        /// Int is increase by sacrificed stats.
        /// </summary>
        bool IncreaseIntBySacrificing { get; set; }

        /// <summary>
        /// Wis is increase by sacrificed stats.
        /// </summary>
        bool IncreaseWisBySacrificing { get; set; }

        /// <summary>
        /// Luc is increase by sacrificed stats.
        /// </summary>
        bool IncreaseLucBySacrificing { get; set; }

        /// <summary>
        /// % for physical defence.
        /// </summary>
        int DefencePersent { get; set; }

        /// <summary>
        /// % for magic defence.
        /// </summary>
        int ResistancePersent { get; set; }

        /// <summary>
        /// Saves autostats to db.
        /// </summary>
        Task<bool> TrySetAutoStats(byte str, byte dex, byte rec, byte intl, byte wis, byte luc);

        /// <summary>
        /// Possibility to hit enemy.
        /// </summary>
        double PhysicalHittingChance { get; }

        /// <summary>
        /// Possibility to escape hit.
        /// </summary>
        double PhysicalEvasionChance { get; }

        /// <summary>
        /// Possibility to shot enemy.
        /// </summary>
        double ShootingHittingChance { get; }

        /// <summary>
        /// Possibility to escape shot.
        /// </summary>
        double ShootingEvasionChance { get; }

        /// <summary>
        /// Exclusive for fighters. There is skill "Fleet Foot", that provides possibility to escape shot regardless dex value.
        /// </summary>
        byte ConstShootingEvasionChance { get; set; }

        /// <summary>
        /// Possibility to make critical hit.
        /// </summary>
        double CriticalHittingChance { get; }

        /// <summary>
        /// Possibility to hit enemy.
        /// </summary>
        double MagicHittingChance { get; }

        /// <summary>
        /// Possibility to escape hit.
        /// </summary>
        double MagicEvasionChance { get; }

        /// <summary>
        /// Possibility to hit enemy gained from skills.
        /// </summary>
        int ExtraPhysicalHittingChance { get; set; }

        /// <summary>
        /// Possibility to escape hit gained from skills.
        /// </summary>
        int ExtraPhysicalEvasionChance { get; set; }

        /// <summary>
        /// Possibility to shoot enemy gained from skills.
        /// </summary>
        int ExtraShootingHittingChance { get; set; }

        /// <summary>
        /// Possibility to escape shoot gained from skills.
        /// </summary>
        int ExtraShootingEvasionChance { get; set; }

        /// <summary>
        /// Possibility to make critical hit.
        /// </summary>
        int ExtraCriticalHittingChance { get; set; }

        /// <summary>
        /// Possibility to hit enemy gained from skills.
        /// </summary>
        int ExtraMagicHittingChance { get; set; }

        /// <summary>
        /// Possibility to escape hit gained from skills.
        /// </summary>
        int ExtraMagicEvasionChance { get; set; }

        /// <summary>
        /// Additional attack power.
        /// </summary>
        int ExtraPhysicalAttackPower { get; set; }

        /// <summary>
        /// Additional attack power.
        /// </summary>
        int ExtraMagicAttackPower { get; set; }

        /// <summary>
        /// Set by inventory weapon.
        /// </summary>
        byte WeaponType { get; set; }

        /// <summary>
        /// Passive weapon power ups go here.
        /// </summary>
        Dictionary<byte, byte> WeaponAttackPassiveSkillModificator { get; }

        /// <summary>
        /// Passive shield power up.
        /// </summary>
        byte ShieldDefencePassiveSkillModificator { get; set; }

        /// <summary>
        /// Min attack from weapon.
        /// </summary>
        int WeaponMinAttack { get; set; }

        /// <summary>
        /// Max attack from weapon.
        /// </summary>
        int WeaponMaxAttack { get; set; }

        /// <summary>
        /// Min physical attack.
        /// </summary>
        int MinAttack { get; }

        /// <summary>
        /// Max physical attack.
        /// </summary>
        int MaxAttack { get; }

        /// <summary>
        /// Min magic attack.
        /// </summary>
        int MinMagicAttack { get; }

        /// <summary>
        /// Max magic attack.
        /// </summary>
        int MaxMagicAttack { get; }

        /// <summary>
        /// Absorbs damage regardless of REC value.
        /// </summary>
        ushort Absorption { get; set; }

        /// <summary>
        /// Free stat points, that player can set.
        /// </summary>
        ushort StatPoint { get; }

        /// <summary>
        /// Tries to set const stats.
        /// </summary>
        bool TrySetStats(ushort? str = null, ushort? dex = null, ushort? rec = null, ushort? intl = null, ushort? wis = null, ushort? luc = null, ushort? statPoints = null);

        /// <summary>
        /// Increases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        void IncreasePrimaryStat(ushort amount = 1);

        /// <summary>
        /// Decreases a character's main stat by a certain amount
        /// </summary>
        /// <param name="amount">Decrease amount</param>
        void DecreasePrimaryStat(ushort amount = 1);

        /// <summary>
        /// Initiates <see cref="OnAdditionalStatsUpdate"/>
        /// </summary>
        void RaiseAdditionalStatsUpdate();

        /// <summary>
        /// Initiates <see cref="OnResetStats"/>
        /// </summary>
        void RaiseResetStats();

        /// <summary>
        /// Triggers const stats update send to player.
        /// </summary>
        event Action OnResetStats;

        /// <summary>
        /// Triggers additional stats update send to player. Trigger it via <see cref="RaiseAdditionalStatsUpdate"/>
        /// </summary>
        event Action OnAdditionalStatsUpdate;

        /// <summary>
        /// Event, that is fired, when rec constant or extra stat changes, needed for max hp calculation.
        /// </summary>
        event Action OnRecUpdate;

        /// <summary>
        /// Event, that is fired, when dex constant stat or extra changes, needed for max sp calculation.
        /// </summary>
        event Action OnDexUpdate;

        /// <summary>
        /// Event, that is fired, when wis constant stat or extra changes, needed for max mp calculation.
        /// </summary>
        event Action OnWisUpdate;

    }
}
