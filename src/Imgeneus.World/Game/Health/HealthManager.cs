using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Health
{
    public class HealthManager : IHealthManager
    {
        private readonly ILogger<HealthManager> _logger;
        private readonly IStatsManager _statsManager;
        private readonly ILevelProvider _levelProvider;
        private readonly ICharacterConfiguration _characterConfiguration;
        private readonly IDatabase _database;
        private int _ownerId;

        public HealthManager(ILogger<HealthManager> logger, IStatsManager statsManager, ILevelProvider levelProvider, ICharacterConfiguration characterConfiguration, IDatabase database)
        {
            _logger = logger;
            _statsManager = statsManager;
            _levelProvider = levelProvider;
            _characterConfiguration = characterConfiguration;
            _database = database;
            _statsManager.OnRecUpdate += StatsManager_OnRecUpdate;
            _statsManager.OnDexUpdate += StatsManager_OnDexUpdate;
            _statsManager.OnWisUpdate += StatsManager_OnWisUpdate;

#if DEBUG
            _logger.LogDebug("HealthManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~HealthManager()
        {
            _logger.LogDebug("HealthManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(int id, int currentHP, int currentSP, int currentMP, int? constHP, int? constSP, int? constMP, CharacterProfession? profession)
        {
            _ownerId = id;

            Class = profession;

            _constHP = constHP;
            _constMP = constMP;
            _constSP = constSP;

            CurrentHP = currentHP;
            CurrentSP = currentSP;
            CurrentMP = currentMP;

            ExtraHP = 0;
            ExtraSP = 0;
            ExtraMP = 0;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                return;

            character.HealthPoints = (ushort)CurrentHP;
            character.ManaPoints = (ushort)CurrentMP;
            character.StaminaPoints = (ushort)CurrentSP;

            await _database.SaveChangesAsync();
        } 

        public void Dispose()
        {
            _statsManager.OnRecUpdate -= StatsManager_OnRecUpdate;
            _statsManager.OnDexUpdate -= StatsManager_OnDexUpdate;
            _statsManager.OnWisUpdate -= StatsManager_OnWisUpdate;
        }

        #endregion

        #region Max
        public int MaxHP => ConstHP + ExtraHP + ReactionExtraHP;

        public int MaxSP => ConstSP + ExtraSP + DexterityExtraSP;

        public int MaxMP => ConstMP + ExtraMP + WisdomExtraMP;

        #endregion

        #region Current
        private int _currentHP;
        public int CurrentHP
        {
            get => _currentHP;
            protected set
            {
                if (_currentHP == value)
                    return;

                if (value > MaxHP)
                    value = MaxHP;

                var oldHP = _currentHP;
                _currentHP = value;
                if (_currentHP <= 0)
                {
                    _currentHP = 0;
                    //IsDead = true;
                }

                //if (_currentHP == MaxHP)
                //DamageMakers.Clear();

                HP_Changed?.Invoke(_ownerId, new HitpointArgs(oldHP, _currentHP));
            }
        }

        private int _currentSP;
        public int CurrentSP
        {
            get => _currentSP;
            set
            {
                if (_currentSP == value)
                    return;

                if (value > MaxSP)
                    value = MaxSP;

                var args = new HitpointArgs(_currentSP, value);
                _currentSP = value;
                SP_Changed?.Invoke(_ownerId, args);
            }
        }

        private int _currentMP;
        public int CurrentMP
        {
            get => _currentMP;
            set
            {
                if (_currentMP == value)
                    return;

                if (value > MaxMP)
                    value = MaxMP;

                var args = new HitpointArgs(_currentMP, value);
                _currentMP = value;
                MP_Changed?.Invoke(_ownerId, args);
            }
        }

        #endregion

        #region Extras

        private int _extraHP;
        public int ExtraHP
        {
            get => _extraHP;
            set
            {
                _extraHP = value;
                OnMaxHPChanged?.Invoke(_ownerId, MaxHP);
            }
        }

        private int _extraSP;
        public int ExtraSP
        {
            get => _extraSP;
            set
            {
                _extraSP = value;
                OnMaxSPChanged?.Invoke(_ownerId, MaxSP);
            }
        }

        private int _extraMP;
        public int ExtraMP
        {
            get => _extraMP;
            set
            {
                _extraMP = value;
                OnMaxMPChanged?.Invoke(_ownerId, MaxMP);
            }
        }

        #endregion

        #region Consts

        public CharacterProfession? Class { get; private set; }

        private int? _constHP;
        public int ConstHP
        {
            get
            {
                if (_constHP.HasValue)
                    return _constHP.Value;

                var level = _levelProvider.Level <= 60 ? _levelProvider.Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constHP = _characterConfiguration.GetConfig(index).HP;

                return constHP;

            }
            private set => _constHP = value;
        }


        private int? _constSP;
        public int ConstSP
        {
            get
            {
                if (_constSP.HasValue)
                    return _constSP.Value;

                var level = _levelProvider.Level <= 60 ? _levelProvider.Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constSP = _characterConfiguration.GetConfig(index).SP;

                return constSP;
            }

            private set => _constSP = value;
        }


        private int? _constMP;
        public int ConstMP
        {

            get
            {
                if (_constMP.HasValue)
                    return _constMP.Value;

                var level = _levelProvider.Level <= 60 ? _levelProvider.Level : 60;
                var index = (level - 1) * 6 + (byte)Class;
                var constMP = _characterConfiguration.GetConfig(index).MP;

                return constMP;
            }

            private set => _constMP = value;
        }

        #endregion

        #region Recover

        public void Recover(int hp, int mp, int sp)
        {
            IncreaseHP(hp);
            CurrentMP += mp;
            CurrentSP += sp;

            OnRecover?.Invoke(_ownerId, CurrentHP, CurrentMP, CurrentSP);
        }

        public void FullRecover()
        {
            Recover(MaxHP, MaxMP, MaxSP);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Extra HP given by Reaction formula
        /// </summary>
        private int ReactionExtraHP => _statsManager.TotalRec * 5;

        /// <summary>
        /// Extra MP given by Wisdom formula
        /// </summary>
        private int WisdomExtraMP => _statsManager.TotalWis * 5;

        /// <summary>
        /// Extra SP given by Dexterity formula
        /// </summary>
        private int DexterityExtraSP => _statsManager.TotalDex * 5;

        private void StatsManager_OnRecUpdate() => OnMaxHPChanged?.Invoke(_ownerId, MaxHP);

        private void StatsManager_OnWisUpdate() => OnMaxMPChanged?.Invoke(_ownerId, MaxMP);

        private void StatsManager_OnDexUpdate() => OnMaxSPChanged?.Invoke(_ownerId, MaxSP);
        
        #endregion

        #region Events

        public event Action<int, int> OnMaxHPChanged;

        public event Action<int, int> OnMaxSPChanged;

        public event Action<int, int> OnMaxMPChanged;

        public event Action<int, int, int, int> OnRecover;

        #endregion

        public void DecreaseHP(int hp, IKiller damageMaker)
        {
            if (hp == 0)
                return;

            /*if (DamageMakers.ContainsKey(damageMaker))
                DamageMakers[damageMaker] += hp;
            else
                DamageMakers.TryAdd(damageMaker, hp);

            CurrentHP -= hp;
            DecreaseHP(damageMaker);*/
        }

        public void IncreaseHP(int hp)
        {
            if (hp == 0)
                return;

            CurrentHP += hp;
        }

        /// <summary>
        /// Event, that is fired, when hp changes.
        /// </summary>
        public event Action<int, HitpointArgs> HP_Changed;

        /// <summary>
        /// Event, that is fired, when mp changes.
        /// </summary>
        public event Action<int, HitpointArgs> MP_Changed;

        /// <summary>
        /// Event, that is fired, when sp changes.
        /// </summary>
        public event Action<int, HitpointArgs> SP_Changed;
    }
}
