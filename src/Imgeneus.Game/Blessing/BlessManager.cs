using Imgeneus.Database.Entities;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.Game.Blessing
{
    /// <summary>
    /// Goddess bless, gained by killing enemies and destroying altars.
    /// </summary>
    public class BlessManager : IBlessManager
    {
        private object _syncObject = new object();
        public const int MAX_AMOUNT_VALUE = 12288;

        public BlessManager()
        {
            _fullBlessTimer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
            _fullBlessTimer.AutoReset = false;
            _fullBlessTimer.Elapsed += FullBlessTimer_Elapsed;
        }

        #region Amount

        private int _lightAmount;

        public event Action<BlessArgs> OnLightBlessChanged;

        public int LightAmount
        {
            get
            {
                return _lightAmount;
            }
            set
            {
                lock (_syncObject)
                {
                    if (IsFullBless)
                        return;

                    var oldValue = _lightAmount;

                    if (value > 0)
                        _lightAmount = value;
                    else
                        _lightAmount = 0;

                    if (_lightAmount >= MAX_AMOUNT_VALUE)
                    {
                        _lightAmount = MAX_AMOUNT_VALUE;
                        StartFullBless(Fraction.Light);
                    }

                    OnLightBlessChanged?.Invoke(new BlessArgs(oldValue, _lightAmount));
                }
            }
        }

        private int _darkAmount;

        public event Action<BlessArgs> OnDarkBlessChanged;

        public int DarkAmount
        {
            get
            {
                return _darkAmount;
            }
            set
            {
                lock (_syncObject)
                {
                    if (IsFullBless)
                        return;

                    var oldValue = _darkAmount;

                    if (value > 0)
                        _darkAmount = value;
                    else
                        _darkAmount = 0;

                    if (_darkAmount > MAX_AMOUNT_VALUE)
                    {
                        _darkAmount = MAX_AMOUNT_VALUE;
                        StartFullBless(Fraction.Dark);
                    }

                    OnDarkBlessChanged?.Invoke(new BlessArgs(oldValue, _darkAmount));
                }
            }
        }

        #endregion

        #region Full bless

        private readonly Timer _fullBlessTimer = new Timer();

        /// <summary>
        /// When full bless will end.
        /// </summary>
        public DateTime FullBlessingEnd { get; private set; }

        public uint RemainingTime
        {
            get
            {
                lock (_syncObject)
                {
                    if (IsFullBless)
                        return (uint)FullBlessingEnd.Subtract(DateTime.UtcNow).TotalMilliseconds;
                    else
                        return 0;
                }
            }
        }

        /// <summary>
        /// Indicates if now full bless is running.
        /// </summary>
        public bool IsFullBless { get; private set; }

        /// <summary>
        /// Starts full bless for some fraction.
        /// </summary>
        private void StartFullBless(Fraction fraction)
        {
            IsFullBless = true;

            if (fraction == Fraction.Light)
                DarkAmount = 0;

            if (fraction == Fraction.Dark)
                LightAmount = 0;

            FullBlessingEnd = DateTime.UtcNow.AddMinutes(10);
            _fullBlessTimer.Start();
        }

        private void FullBlessTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_syncObject)
            {
                IsFullBless = false;
                DarkAmount = 0;
                LightAmount = 0;
            }
        }

        #endregion
    }
}
