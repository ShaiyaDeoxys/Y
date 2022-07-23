using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Country;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        private void OnDarkBlessChanged(BlessArgs args)
        {
            if (CountryProvider.Country == CountryType.Dark)
            {
                AddBlessBonuses(args);
                _packetFactory.SendBlessUpdate(GameSession.Client, CountryProvider.Country, args.NewValue);
            }
        }

        private void OnLightBlessChanged(BlessArgs args)
        {
            if (CountryProvider.Country == CountryType.Light)
            {
                AddBlessBonuses(args);
                _packetFactory.SendBlessUpdate(GameSession.Client, CountryProvider.Country, args.NewValue);
            }
        }

        private int blessExtraHP;
        private int blessExtraMP;
        private int blessExtraSP;
        private int blessPhysicalDef;
        private int blessMagicDef;

        /// <summary>
        /// Sends update of bonuses, based on bless amount change.
        /// </summary>
        /// <param name="args">bless args</param>
        private void AddBlessBonuses(BlessArgs args)
        {
            // Max HP, MP, SP.
            if (args.OldValue >= IBlessManager.MAX_HP_SP_MP && args.NewValue < IBlessManager.MAX_HP_SP_MP)
            {
                HealthManager.ExtraHP -= blessExtraHP;
                HealthManager.ExtraMP -= blessExtraMP;
                HealthManager.ExtraSP -= blessExtraSP;
            }
            if (args.OldValue < IBlessManager.MAX_HP_SP_MP && args.NewValue >= IBlessManager.MAX_HP_SP_MP)
            {
                blessExtraHP = HealthManager.MaxHP / 5;
                blessExtraMP = HealthManager.MaxMP / 5;
                blessExtraSP = HealthManager.MaxSP / 5;

                HealthManager.ExtraHP += blessExtraHP;
                HealthManager.ExtraMP += blessExtraMP;
                HealthManager.ExtraSP += blessExtraSP;
            }

            // Physical defence.
            if (args.OldValue >= IBlessManager.PHYSICAL_DEFENCE && args.NewValue < IBlessManager.PHYSICAL_DEFENCE)
            {
                StatsManager.ExtraDefense -= blessPhysicalDef;
            }

            if (args.OldValue < IBlessManager.PHYSICAL_DEFENCE && args.NewValue >= IBlessManager.PHYSICAL_DEFENCE)
            {
                blessPhysicalDef = StatsManager.TotalDefense / 10;

                StatsManager.ExtraDefense += blessPhysicalDef;
            }

            // Magic defence.
            if (args.OldValue >= IBlessManager.SHOOTING_MAGIC_DEFENCE && args.NewValue < IBlessManager.SHOOTING_MAGIC_DEFENCE)
            {
                StatsManager.ExtraResistance -= blessMagicDef;
            }

            if (args.OldValue < IBlessManager.SHOOTING_MAGIC_DEFENCE && args.NewValue >= IBlessManager.SHOOTING_MAGIC_DEFENCE)
            {
                blessMagicDef = StatsManager.TotalResistance / 10;

                StatsManager.ExtraResistance += blessMagicDef;
            }

            // Stats.
            if (args.OldValue >= IBlessManager.STATS && args.NewValue < IBlessManager.STATS)
            {
                StatsManager.ExtraStr -= 50;
                StatsManager.ExtraDex -= 50;
                StatsManager.ExtraRec -= 50;
                StatsManager.ExtraInt -= 50;
                StatsManager.ExtraWis -= 50;
                StatsManager.ExtraLuc -= 50;
            }

            if (args.OldValue < IBlessManager.STATS && args.NewValue >= IBlessManager.STATS)
            {
                StatsManager.ExtraStr += 50;
                StatsManager.ExtraDex += 50;
                StatsManager.ExtraRec += 50;
                StatsManager.ExtraInt += 50;
                StatsManager.ExtraWis += 50;
                StatsManager.ExtraLuc += 50;
            }

            // Critical hit.
            if (args.OldValue >= IBlessManager.FULL_BLESS_BONUS && args.NewValue < IBlessManager.FULL_BLESS_BONUS)
            {
                StatsManager.ExtraCriticalHittingChance -= 20;
                StatsManager.ExtraPhysicalHittingChance -= 10;
                StatsManager.ExtraPhysicalEvasionChance -= 10;
                StatsManager.ExtraMagicHittingChance -= 10;
                StatsManager.ExtraMagicEvasionChance -= 10;
                StatsManager.ExtraShootingHittingChance -= 10;
                StatsManager.ExtraShootingEvasionChance -= 10;
            }

            if (args.OldValue < IBlessManager.FULL_BLESS_BONUS && args.NewValue >= IBlessManager.FULL_BLESS_BONUS)
            {
                StatsManager.ExtraCriticalHittingChance += 20;
                StatsManager.ExtraPhysicalHittingChance += 10;
                StatsManager.ExtraPhysicalEvasionChance += 10;
                StatsManager.ExtraMagicHittingChance += 10;
                StatsManager.ExtraMagicEvasionChance += 10;
                StatsManager.ExtraShootingHittingChance += 10;
                StatsManager.ExtraShootingEvasionChance += 10;
            }
        }


        private void IncreaseBless(uint senderId, IKiller killer)
        {
            if (killer is Character && killer.CountryProvider.Country != CountryProvider.Country)
            {
                if (killer.CountryProvider.Country == CountryType.Light)
                    BlessManager.LightAmount += 10;

                if (killer.CountryProvider.Country == CountryType.Dark)
                    BlessManager.DarkAmount += 10;
            }
        }
    }
}
