using Imgeneus.Database;
using Imgeneus.Database.Entities;
using Imgeneus.World.Game.AdditionalInfo;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Player.Config;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
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
        private readonly IAdditionalInfoManager _additionalInfoManager;
        private uint _ownerId;

        public HealthManager(ILogger<HealthManager> logger, IStatsManager statsManager, ILevelProvider levelProvider, ICharacterConfiguration characterConfiguration, IDatabase database, IAdditionalInfoManager additionalInfoManager)
        {
            _logger = logger;
            _statsManager = statsManager;
            _levelProvider = levelProvider;
            _characterConfiguration = characterConfiguration;
            _database = database;
            _additionalInfoManager = additionalInfoManager;
            _statsManager.OnRecUpdate += StatsManager_OnRecUpdate;
            _statsManager.OnDexUpdate += StatsManager_OnDexUpdate;
            _statsManager.OnWisUpdate += StatsManager_OnWisUpdate;
            _levelProvider.OnLevelUp += OnLevelUp;

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

        public void Init(uint id, int currentHP, int currentSP, int currentMP, int? constHP, int? constSP, int? constMP)
        {
            _ownerId = id;

            _constHP = constHP;
            _constMP = constMP;
            _constSP = constSP;

            CurrentHP = currentHP;
            CurrentSP = currentSP;
            CurrentMP = currentMP;
        }

        public async Task Clear()
        {
            var character = await _database.Characters.FindAsync(_ownerId);
            if (character is null)
                return;

            character.HealthPoints = CurrentHP;
            character.ManaPoints = CurrentMP;
            character.StaminaPoints = CurrentSP;

            await _database.SaveChangesAsync();
        }

        public void Dispose()
        {
            _statsManager.OnRecUpdate -= StatsManager_OnRecUpdate;
            _statsManager.OnDexUpdate -= StatsManager_OnDexUpdate;
            _statsManager.OnWisUpdate -= StatsManager_OnWisUpdate;
            _levelProvider.OnLevelUp -= OnLevelUp;
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

                if (value < 0)
                    value = 0;

                var oldHP = _currentHP;
                _currentHP = value;
                if (_currentHP <= 0)
                {
                    _currentHP = 0;
                    IsDead = true;
                }

                if (_currentHP == MaxHP)
                    DamageMakers.Clear();

                HP_Changed?.Invoke(_ownerId, new HitpointArgs(oldHP, _currentHP));
            }
        }

        public event Action<uint, IKiller, int> OnGotDamage;

        public void DecreaseHP(int hp, IKiller damageMaker)
        {
            if (hp == 0 || CurrentHP == 0)
                return;

            //_logger.LogDebug("HealthManager {hashcode} got damage {damage}", GetHashCode(), hp);

            if (damageMaker is not null)
            {
                if (DamageMakers.ContainsKey(damageMaker))
                    DamageMakers[damageMaker] += hp;
                else
                    DamageMakers.TryAdd(damageMaker, hp);
            }

            CurrentHP -= hp;
            OnGotDamage?.Invoke(_ownerId, damageMaker, hp);
        }

        public void IncreaseHP(int hp)
        {
            if (hp == 0)
                return;

            CurrentHP += hp;
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

                if (value < 0)
                    value = 0;

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

                if (value < 0)
                    value = 0;

                var args = new HitpointArgs(_currentMP, value);
                _currentMP = value;
                MP_Changed?.Invoke(_ownerId, args);
            }
        }

        public void InvokeUsedMPSP(ushort usedMP, ushort usedSP)
        {
            MP_SP_Used?.Invoke(usedMP, usedSP);
        }

        public void RaiseHitpointsChange()
        {
            OnCurrentHitpointsChanged?.Invoke(CurrentHP, CurrentMP, CurrentSP);
        }

        public event Action<int, int, int> OnCurrentHitpointsChanged;

        #endregion

        #region Extras

        private int _extraHP;
        public int ExtraHP
        {
            get => _extraHP;
            set
            {
                _extraHP = value;

                if (CurrentHP > MaxHP)
                {
                    CurrentHP = MaxHP;
                    RaiseHitpointsChange();
                }

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

                if (CurrentSP > MaxSP)
                {
                    CurrentSP = MaxSP;
                    RaiseHitpointsChange();
                }

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

                if (CurrentMP > MaxMP)
                {
                    CurrentMP = MaxMP;
                    RaiseHitpointsChange();
                }

                OnMaxMPChanged?.Invoke(_ownerId, MaxMP);
            }
        }

        #endregion

        #region Consts

        private int? _constHP;
        public int ConstHP
        {
            get
            {
                if (_constHP.HasValue)
                    return _constHP.Value;

                var level = _levelProvider.Level <= 60 ? _levelProvider.Level : 60;
                var index = (level - 1) * 6 + (byte)_additionalInfoManager.Class;
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
                var index = (level - 1) * 6 + (byte)_additionalInfoManager.Class;
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
                var index = (level - 1) * 6 + (byte)_additionalInfoManager.Class;
                var constMP = _characterConfiguration.GetConfig(index).MP;

                return constMP;
            }

            private set => _constMP = value;
        }

        #endregion

        #region Recover

        public void Recover(int hp, int mp, int sp)
        {
            if (CurrentHP + hp > MaxHP)
                hp = MaxHP - CurrentHP;

            if (CurrentMP + mp > MaxMP)
                mp = MaxMP - CurrentMP;

            if (CurrentSP + sp > MaxSP)
                sp = MaxSP - CurrentSP;

            IncreaseHP(hp);
            CurrentMP += mp;
            CurrentSP += sp;

            OnRecover?.Invoke(_ownerId, CurrentHP, CurrentMP, CurrentSP);
        }

        public void FullRecover()
        {
            Recover(MaxHP, MaxMP, MaxSP);
        }

        private void OnLevelUp(uint senderId, ushort level, ushort oldLevel)
        {
            OnMaxHPChanged?.Invoke(_ownerId, MaxHP);
            OnMaxMPChanged?.Invoke(_ownerId, MaxMP);
            OnMaxSPChanged?.Invoke(_ownerId, MaxSP);

            FullRecover();
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

        public event Action<uint, int> OnMaxHPChanged;

        public event Action<uint, int> OnMaxSPChanged;

        public event Action<uint, int> OnMaxMPChanged;

        public event Action<uint, int, int, int> OnRecover;

        #endregion

        #region Death

        public event Action<uint, IKiller> OnDead;

        private bool _isDead;

        public bool IsDead
        {
            get => _isDead;
            protected set
            {
                _isDead = value;

                if (_isDead)
                {
                    //_logger.LogDebug("HealthManager {hashcode} is dead", GetHashCode());

                    var killer = MaxDamageMaker;
                    OnDead?.Invoke(_ownerId, killer);
                    DamageMakers.Clear();
                }
            }
        }


        /// <summary>
        /// Collection of entities, that made damage to this killable.
        /// </summary>
        public ConcurrentDictionary<IKiller, int> DamageMakers { get; private set; } = new ConcurrentDictionary<IKiller, int>();

        public IKiller MaxDamageMaker
        {
            get
            {
                IKiller maxDamageMaker = null;
                int damage = 0;
                foreach (var dmg in DamageMakers)
                {
                    if (dmg.Value > damage)
                    {
                        damage = dmg.Value;
                        maxDamageMaker = dmg.Key;
                    }
                }

                return maxDamageMaker;
            }
        }

        #endregion

        #region Rebirth

        public event Action<uint> OnRebirthed;

        public void Rebirth()
        {
            FullRecover();
            IsDead = false;

            OnRebirthed?.Invoke(_ownerId);
        }

        #endregion

        #region IsAttackable?

        private bool _isAttackable = true;
        public bool IsAttackable
        {
            get => _isAttackable;
            set
            {
                _isAttackable = value;
                OnIsAttackableChanged?.Invoke(_isAttackable);
            }
        }

        public event Action<bool> OnIsAttackableChanged;

        #endregion

        #region Reflect damage

        public bool ReflectPhysicDamage { get; set; }

        public bool ReflectMagicDamage { get; set; }

        public event Action<uint, uint, Damage> OnMirrowDamage;

        public void InvokeMirrowDamage(Damage damage, IKillable damageMaker)
        {
            DecreaseHP(damage.HP, damageMaker as IKiller);
            CurrentSP -= damage.SP;
            CurrentMP -= damage.MP;

            OnMirrowDamage?.Invoke(_ownerId, damageMaker.Id, damage);
        }

        #endregion

        #region Use instead of HP

        public bool UseMPInsteadOfHP { get; set; }

        public bool UseSPInsteadOfHP { get; set; }

        #endregion

        public event Action<uint, HitpointArgs> HP_Changed;

        public event Action<uint, HitpointArgs> MP_Changed;

        public event Action<uint, HitpointArgs> SP_Changed;

        public event Action<ushort, ushort> MP_SP_Used;
    }
}
