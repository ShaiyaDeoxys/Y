using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Kills
{
    public class KillsManager : IKillsManager
    {
        private readonly ILogger<KillsManager> _logger;
        private readonly IDatabase _database;
        private readonly IHealthManager _healthManager;
        private readonly ICountryProvider _countryProvider;
        private readonly IMapProvider _mapProvider;
        private readonly IMovementManager _movementManager;
        private uint _ownerId;

        public KillsManager(ILogger<KillsManager> logger, IDatabase database, IHealthManager healthManager, ICountryProvider countryProvider, IMapProvider mapProvider, IMovementManager movementManager)
        {
            _logger = logger;
            _database = database;
            _healthManager = healthManager;
            _countryProvider = countryProvider;
            _mapProvider = mapProvider;
            _movementManager = movementManager;
            _healthManager.OnDead += HealthManager_OnDead;
#if DEBUG
            _logger.LogDebug("KillsManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~KillsManager()
        {
            _logger.LogDebug("KillsManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId, ushort kills = 0, ushort deaths = 0, ushort victories = 0, ushort defeats = 0)
        {
            _ownerId = ownerId;

            Kills = kills;
            Deaths = deaths;
            Victories = victories;
            Defeats = defeats;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
            {
                _logger.LogError("Character {id} is not found in database.", _ownerId);
                return;
            }

            character.Kills = Kills;
            character.Deaths = Deaths;
            character.Victories = Victories;
            character.Defeats = Defeats;

            await _database.SaveChangesAsync();
        }

        public void Dispose()
        {
            _healthManager.OnDead -= HealthManager_OnDead;
        }

        #endregion

        private ushort _kills;
        public ushort Kills
        {
            get => _kills;
            set
            {
                _kills = value;
                OnKillsChanged?.Invoke(_ownerId, _kills);
                OnCountChanged?.Invoke(0, _kills);
            }
        }
        public event Action<uint, ushort> OnKillsChanged;

        private void HealthManager_OnDead(uint senderId, IKiller killer)
        {
            if (killer is Character killerCharacter &&
                killer.CountryProvider.Country != _countryProvider.Country &&
                killerCharacter.MapProvider.Map is not null &&
                killerCharacter.MapProvider.Map.Id != 40)
            {
                if (killerCharacter.PartyManager.HasParty)
                {
                    foreach (var member in killerCharacter.PartyManager.Party.Members.Where(x => x.Map == _mapProvider.Map && MathExtensions.Distance(x.PosX, _movementManager.PosX, x.PosZ, _movementManager.PosZ) <= 100).ToList())
                        member.KillsManager.Kills++;
                }
                else
                {
                    killerCharacter.KillsManager.Kills++;
                }

                Deaths++;
            }

        }

        private ushort _deaths;
        public ushort Deaths
        {
            get => _deaths;
            set
            {
                _deaths = value;
                OnCountChanged?.Invoke(1, _kills);
            }
        }

        private ushort _victories;
        public ushort Victories
        {
            get => _victories; set
            {

                _victories = value;
                OnCountChanged?.Invoke(2, _victories);
            }
        }

        public ushort _defeats;
        public ushort Defeats
        {
            get => _defeats;
            set
            {
                _defeats = value;
                OnCountChanged?.Invoke(3, _victories);
            }
        }

        public event Action<byte, ushort> OnCountChanged;
    }
}
