using Imgeneus.Core.Extensions;
using Imgeneus.Database;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly ILevelProvider _levelProvider;
        private readonly IStatsManager _statsManager;
        private readonly IInventoryManager _inventoryManager;
        private uint _ownerId;

        public KillsManager(ILogger<KillsManager> logger, IDatabase database, IHealthManager healthManager, ICountryProvider countryProvider, IMapProvider mapProvider, IMovementManager movementManager, ILevelProvider levelProvider, IStatsManager statsManager, IInventoryManager inventoryManager)
        {
            _logger = logger;
            _database = database;
            _healthManager = healthManager;
            _countryProvider = countryProvider;
            _mapProvider = mapProvider;
            _movementManager = movementManager;
            _levelProvider = levelProvider;
            _statsManager = statsManager;
            _inventoryManager = inventoryManager;
            _healthManager.OnDead += HealthManager_OnDead;
            _statsManager.OnResetStats += StatsManager_OnResetStats;

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

        public void Init(uint ownerId, uint kills = 0, uint deaths = 0, uint victories = 0, uint defeats = 0, byte killLevel = 1, byte deathLevel = 1)
        {
            _ownerId = ownerId;

            Kills = kills;
            Deaths = deaths;
            Victories = victories;
            Defeats = defeats;
            KillLevel = killLevel;
            DeathLevel = deathLevel;
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
            character.KillLevel = KillLevel;
            character.DeathLevel = DeathLevel;

            await _database.SaveChangesAsync();
        }

        public void Dispose()
        {
            _healthManager.OnDead -= HealthManager_OnDead;
            _statsManager.OnResetStats -= StatsManager_OnResetStats;
        }

        #endregion

        private uint _kills;
        public uint Kills
        {
            get => _kills;
            set
            {
                _kills = value;
                OnKillsChanged?.Invoke(_ownerId, _kills);
                OnCountChanged?.Invoke(0, _kills);
            }
        }
        public event Action<uint, uint> OnKillsChanged;

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

        private uint _deaths;
        public uint Deaths
        {
            get => _deaths;
            set
            {
                _deaths = value;
                OnCountChanged?.Invoke(1, _deaths);
            }
        }

        private uint _victories;
        public uint Victories
        {
            get => _victories;
            set
            {

                _victories = value;
                OnCountChanged?.Invoke(2, _victories);
            }
        }

        public uint _defeats;
        public uint Defeats
        {
            get => _defeats;
            set
            {
                _defeats = value;
                OnCountChanged?.Invoke(3, _defeats);
            }
        }

        public event Action<byte, uint> OnCountChanged;

        #region Vet rewards

        public byte KillLevel { get; set; }

        public byte DeathLevel { get; set; }

        public (bool Ok, ushort Stats) TryGetKillsReward()
        {
            if (_levelProvider.Level < 16)
                return (false, 0);

            if (!_killsRank.ContainsKey(KillLevel) || Kills < _killsRank[KillLevel])
                return (false, 0);

            var freeStats = _killsStat[KillLevel];
            _statsManager.TrySetStats(statPoints: (ushort)(_statsManager.StatPoint + freeStats));
            KillLevel++;

            return (true, freeStats);
        }

        public (bool Ok, uint Money) TryGetDeathsReward()
        {
            if (!_deathsRank.ContainsKey(DeathLevel) || Deaths < _deathsRank[DeathLevel])
                return (false, 0);

            var money = _deathsMoney[DeathLevel];
            _inventoryManager.Gold += money;
            DeathLevel++;

            return (true, money);
        }

        private void StatsManager_OnResetStats()
        {
            KillLevel = 1;
        }

        // From: https://shaiya-wiki.eu/en/pvp-rank/
        private readonly Dictionary<byte, uint> _killsRank = new()
        {
            { 1, 1 },
            { 2, 50 },
            { 3, 300 },
            { 4, 1000 },
            { 5, 5000 },
            { 6, 10000 },
            { 7, 20000 },
            { 8, 30000 },
            { 9, 40000 },
            { 10, 50000 },
            { 11, 70000 },
            { 12, 90000 },
            { 13, 110000 },
            { 14, 130000 },
            { 15, 150000 },
            { 16, 200000 },
            { 17, 250000 },
            { 18, 300000 },
            { 19, 350000 },
            { 20, 400000 },
            { 21, 450000 },
            { 22, 500000 },
            { 23, 550000 },
            { 24, 600000 },
            { 25, 650000 },
            { 26, 700000 },
            { 27, 750000 },
            { 28, 800000 },
            { 29, 850000 },
            { 30, 900000 },
            { 31, 1000000 },
        };

        private Dictionary<byte, ushort> _killsStat = new()
        {
            { 1, 1 },
            { 2, 3 },
            { 3, 5 },
            { 4, 7 },
            { 5, 12 },
            { 6, 18 },
            { 7, 20 },
            { 8, 22 },
            { 9, 24 },
            { 10, 26 },
            { 11, 26 },
            { 12, 26 },
            { 13, 27 },
            { 14, 27 },
            { 15, 27 },
            { 16, 28 },
            { 17, 28 },
            { 18, 28 },
            { 19, 28 },
            { 20, 28 },
            { 21, 28 },
            { 22, 29 },
            { 23, 29 },
            { 24, 29 },
            { 25, 29 },
            { 26, 29 },
            { 27, 30 },
            { 28, 30 },
            { 29, 30 },
            { 30, 30 },
            { 31, 30 },
        };

        private readonly Dictionary<byte, uint> _deathsRank = new()
        {
            { 1, 1 },
            { 2, 50 },
            { 3, 300 },
            { 4, 1000 },
            { 5, 5000 },
            { 6, 10000 },
            { 7, 20000 },
            { 8, 30000 },
            { 9, 40000 },
            { 10, 50000 },
            { 11, 70000 },
            { 12, 90000 },
            { 13, 110000 },
            { 14, 130000 },
            { 15, 150000 },
            { 16, 200000 },
            { 17, 250000 },
            { 18, 300000 },
            { 19, 350000 },
            { 20, 400000 },
            { 21, 450000 },
            { 22, 500000 },
            { 23, 550000 },
            { 24, 600000 },
            { 25, 650000 },
            { 26, 700000 },
            { 27, 750000 },
            { 28, 800000 },
            { 29, 850000 },
            { 30, 900000 },
            { 31, 1000000 }
        };

        private Dictionary<byte, uint> _deathsMoney = new()
        {
            { 1, 100 },
            { 2, 200 },
            { 3, 500 },
            { 4, 1000 },
            { 5, 5000 },
            { 6, 10000 },
            { 7, 20000 },
            { 8, 50000 },
            { 9, 100000 },
            { 10, 200000 },
            { 11, 300000 },
            { 12, 400000 },
            { 13, 500000 },
            { 14, 650000 },
            { 15, 800000 },
            { 16, 1000000 },
            { 17, 1200000 },
            { 18, 1400000 },
            { 19, 1600000 },
            { 20, 1800000 },
            { 21, 2000000 },
            { 22, 2200000 },
            { 23, 2400000 },
            { 24, 2600000 },
            { 25, 2800000 },
            { 26, 3000000 },
            { 27, 3200000 },
            { 28, 3400000 },
            { 29, 3600000 },
            { 30, 3800000 },
            { 31, 4000000 },
        };

        #endregion
    }
}
