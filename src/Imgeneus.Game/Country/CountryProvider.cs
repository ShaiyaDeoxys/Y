using Imgeneus.Database.Entities;
using Imgeneus.GameDefinitions.Constants;
using Microsoft.Extensions.Logging;

namespace Imgeneus.World.Game.Country
{
    public class CountryProvider : ICountryProvider
    {
        private readonly ILogger<CountryProvider> _logger;

        private uint _ownerId;

        public CountryProvider(ILogger<CountryProvider> logger)
        {
            _logger = logger;
#if DEBUG
            _logger.LogDebug("CountryProvider {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~CountryProvider()
        {
            _logger.LogDebug("CountryProvider {hashcode} collected by GC", GetHashCode());
        }
#endif
        public CountryType Country { get; private set; }

        public void Init(uint ownerId, Fraction country)
        {
            _ownerId = ownerId;

            switch (country)
            {
                case Fraction.Light:
                    Country = CountryType.Light;
                    break;

                case Fraction.Dark:
                    Country = CountryType.Dark;
                    break;

                default:
                    Country = CountryType.None;
                    break;
            }
        }

        public void Init(uint ownerId, MobFraction country)
        {
            _ownerId = ownerId;

            switch (country)
            {
                case MobFraction.Light:
                    Country = CountryType.Light;
                    break;

                case MobFraction.Dark:
                    Country = CountryType.Dark;
                    break;

                default:
                    Country = CountryType.None;
                    break;
            }
        }

        public CountryType EnemyPlayersFraction
        {
            get
            {
                CountryType playerFraction;
                switch (Country)
                {
                    case CountryType.Dark:
                        playerFraction = CountryType.Light;
                        break;

                    case CountryType.Light:
                        playerFraction = CountryType.Dark;
                        break;

                    default:
                        playerFraction = CountryType.None;
                        break;
                }

                return playerFraction;
            }
        }
    }
}
