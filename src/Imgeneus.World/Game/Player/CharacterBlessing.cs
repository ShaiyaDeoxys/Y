using Imgeneus.World.Game.Blessing;
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

        /// <summary>
        /// Sends update of bonuses, based on bless amount change.
        /// </summary>
        /// <param name="args">bless args</param>
        private void AddBlessBonuses(BlessArgs args)
        {
            if (args.OldValue >= Bless.MAX_HP_SP_MP && args.NewValue < Bless.MAX_HP_SP_MP)
            {
                HealthManager.ExtraHP -= HealthManager.ConstHP / 5;
                HealthManager.ExtraMP -= HealthManager.ConstMP / 5;
                HealthManager.ExtraSP -= HealthManager.ConstSP / 5;
            }
            if (args.OldValue < Bless.MAX_HP_SP_MP && args.NewValue >= Bless.MAX_HP_SP_MP)
            {
                HealthManager.ExtraHP += HealthManager.ConstHP / 5;
                HealthManager.ExtraMP += HealthManager.ConstMP / 5;
                HealthManager.ExtraSP += HealthManager.ConstSP / 5;
            }
        }

    }
}
