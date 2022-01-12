using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;

namespace Imgeneus.World.Game.Health
{
    public class HealthManager : IHealthManager
    {
        private readonly ILogger<HealthManager> _logger;
        private readonly IStatsManager _statsManager;
        private readonly ILevelingManager _levelingManager;

        private int _ownerId;

        public HealthManager(ILogger<HealthManager> logger, IStatsManager statsManager, ILevelingManager levelingManager)
        {
            _logger = logger;
            _statsManager = statsManager;
            _levelingManager = levelingManager;

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

        public void Init(int id, int currentHP, int currentSP, int currentMP)
        {
            _ownerId = id;

            CurrentHP = currentHP;
            CurrentSP = currentSP;
            CurrentMP = currentMP;

            ExtraHP = 0;
            ExtraSP = 0;
            ExtraMP = 0;
        }

        #region Max
        public int MaxHP => _levelingManager.ConstHP + ExtraHP + ReactionExtraHP;

        public int MaxSP => _levelingManager.ConstSP + ExtraSP + DexterityExtraSP;

        public int MaxMP => _levelingManager.ConstMP + ExtraMP + WisdomExtraMP;

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

                //HP_Changed?.Invoke(this, new HitpointArgs(oldHP, _currentHP));
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
                //SP_Changed?.Invoke(this, args);
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
                //MP_Changed?.Invoke(this, args);
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

        #region Helpers

        /// <summary>
        /// Extra HP given by Reaction formula
        /// </summary>
        private int ReactionExtraHP => _statsManager.Reaction * 5;

        /// <summary>
        /// Extra MP given by Wisdom formula
        /// </summary>
        private int WisdomExtraMP => _statsManager.Wisdom * 5;

        /// <summary>
        /// Extra SP given by Dexterity formula
        /// </summary>
        private int DexterityExtraSP => _statsManager.Dexterity * 5;

        #endregion

        #region Events

        public event Action<int, int> OnMaxHPChanged;

        public event Action<int, int> OnMaxSPChanged;

        public event Action<int, int> OnMaxMPChanged;

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
        public event Action<IKillable, HitpointArgs> HP_Changed;

        /// <summary>
        /// Event, that is fired, when mp changes.
        /// </summary>
        public event Action<IKillable, HitpointArgs> MP_Changed;

        /// <summary>
        /// Event, that is fired, when sp changes.
        /// </summary>
        public event Action<IKillable, HitpointArgs> SP_Changed;
    }
}
