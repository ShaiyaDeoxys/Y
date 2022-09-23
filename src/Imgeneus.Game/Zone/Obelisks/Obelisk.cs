using Imgeneus.Database.Entities;
using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Monster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Zone.Obelisks
{
    public class Obelisk : IMapMember, IDisposable
    {
        private readonly ObeliskConfiguration _config;
        private readonly IMobFactory _mobFactory;
        private readonly IBlessManager _blessManager;

        public Obelisk(ObeliskConfiguration config, Map map, IMobFactory mobFactory, IBlessManager blessManager)
        {
            _config = config;
            _mobFactory = mobFactory;
            _blessManager = blessManager;
            Map = map;

            Init();
        }

        #region Map member

        public uint Id => _config.Id;

        public float PosX => _config.PosX;

        public float PosY => _config.PosY;

        public float PosZ => _config.PosZ;

        public ushort Angle => 0;

        public Map Map { get; private set; }

        public int CellId { get; set; }

        public int OldCellId { get; set; }

        #endregion

        private void Init()
        {
            ObeliskCountry = _config.DefaultCountry;

            ushort mobId = 0;
            if (ObeliskCountry == ObeliskCountry.None)
                mobId = _config.NeutralObeliskMobId;
            else if (ObeliskCountry == ObeliskCountry.Light)
                mobId = _config.LightObeliskMobId;
            else if (ObeliskCountry == ObeliskCountry.Dark)
                mobId = _config.DarkObeliskMobId;

            ObeliskAI = _mobFactory.CreateMob(mobId,
                                    false,
                                    new MoveArea(PosX, PosX, PosY, PosY, PosZ, PosZ));

            Map.AddMob(ObeliskAI);
            ObeliskAI.HealthManager.OnDead += ObeliskAI_OnDead;
            InitGuards();

            _rebirthTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            _rebirthTimer.AutoReset = false;
            _rebirthTimer.Elapsed += ObeliskRebirthTimer_Elapsed;
        }

        #region Obelisk

        private Timer _rebirthTimer = new Timer();

        /// <summary>
        /// To whom obelisk belongs to? None, light or dark.
        /// </summary>
        public ObeliskCountry ObeliskCountry { get; private set; }

        /// <summary>
        /// Event is fired, when another fraction has broken obelisk.
        /// </summary>
        public event Action<Obelisk> OnObeliskBroken;

        /// <summary>
        /// Obelisk itself.
        /// </summary>
        public Mob ObeliskAI { get; private set; }

        private void ObeliskAI_OnDead(uint senderId, IKiller killer)
        {
            ObeliskAI.HealthManager.OnDead -= ObeliskAI_OnDead;

            ObeliskCountry = killer.CountryProvider.Country == CountryType.Light ? ObeliskCountry.Light : ObeliskCountry.Dark;
            OnObeliskBroken?.Invoke(this);

            if (ObeliskCountry == ObeliskCountry.Light)
            {
                _blessManager.LightAmount += 1000; // TODO: maybe different value for different obelisks?
                _blessManager.DarkAmount -= 500;
            }
            else
            {
                _blessManager.DarkAmount += 1000;
                _blessManager.LightAmount -= 500;
            }

            ClearGuards();
            _rebirthTimer.Start();
        }

        private void ObeliskRebirthTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_blessManager.IsFullBless)
                return;

            if (ObeliskCountry == ObeliskCountry.Light)
            {
                // Init new ai.
                ObeliskAI = _mobFactory.CreateMob(_config.LightObeliskMobId,
                                    false,
                                    new MoveArea(PosX, PosX, PosY, PosY, PosZ, PosZ));
            }
            else if (ObeliskCountry == ObeliskCountry.Dark)
            {
                // Init new ai.
                ObeliskAI = _mobFactory.CreateMob(_config.DarkObeliskMobId,
                                    false,
                                    new MoveArea(PosX, PosX, PosY, PosY, PosZ, PosZ));
            }

            ObeliskAI.HealthManager.OnDead += ObeliskAI_OnDead;
            Map.AddMob(ObeliskAI);
            InitGuards();
        }


        #endregion

        #region Obelisk guards

        /// <summary>
        /// Obelisk defenders.
        /// </summary>
        public IList<Mob> Guards { get; private set; } = new List<Mob>();

        private readonly List<(Timer Timer, Mob Mob)> _guardRebirthTimers = new List<(Timer, Mob)>();

        private object _syncObjectGuardTimer = new object();

        private void Guard_OnDead(uint senderId, IKiller killer)
        {
            var mob = Guards.First(x => x.Id == senderId);
            mob.HealthManager.OnDead -= Guard_OnDead;

            var rebirthTimer = new Timer();
            rebirthTimer.AutoReset = false;
            rebirthTimer.Interval = mob.RespawnTimeInMilliseconds;
            rebirthTimer.Elapsed += GuardRebirthTimer_Elapsed;
            Guards.Remove(mob);

            lock (_syncObjectGuardTimer)
            {
                _guardRebirthTimers.Add((rebirthTimer, mob));
            }
            rebirthTimer.Start();
        }

        private void GuardRebirthTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;
            var timerMob = _guardRebirthTimers.First(t => t.Timer == timer);
            var mob = timerMob.Mob;
            var shouldAdd = false;

            if (mob.CountryProvider.Country == CountryType.None && ObeliskCountry == ObeliskCountry.None)
            {
                shouldAdd = true;
            }
            else if (mob.CountryProvider.Country == CountryType.Light && ObeliskCountry == ObeliskCountry.Light)
            {
                shouldAdd = true;
            }
            else if (mob.CountryProvider.Country == CountryType.Dark && ObeliskCountry == ObeliskCountry.Dark)
            {
                shouldAdd = true;
            }

            if (shouldAdd)
            {
                var newMob = mob.Clone();
                newMob.HealthManager.OnDead += Guard_OnDead;
                Guards.Add(newMob);
                Map.AddMob(newMob);
            }

            lock (_syncObjectGuardTimer)
            {
                _guardRebirthTimers.Remove(timerMob);
            }
            timer.Elapsed -= GuardRebirthTimer_Elapsed;
        }

        /// <summary>
        /// Creates guards of altar based on altar country.
        /// </summary>
        private void InitGuards()
        {
            foreach (var mob in _config.Mobs)
            {
                for (var i = 0; i < mob.Count; i++)
                {
                    ushort guardId = 0;
                    if (ObeliskCountry == ObeliskCountry.None)
                        guardId = mob.NeutralMobId;
                    else if (ObeliskCountry == ObeliskCountry.Light)
                        guardId = mob.LightMobId;
                    else if (ObeliskCountry == ObeliskCountry.Dark)
                        guardId = mob.DarkMobId;

                    var guardAI = _mobFactory.CreateMob(guardId,
                                          false,
                                          new MoveArea(mob.PosX, mob.PosX, mob.PosY, mob.PosY, mob.PosZ, mob.PosZ));
                    Map.AddMob(guardAI);
                    Guards.Add(guardAI);
                    guardAI.HealthManager.OnDead += Guard_OnDead;
                }
            }
        }

        /// <summary>
        /// Removes guards from map and clears their timers.
        /// </summary>
        private void ClearGuards()
        {
            foreach (var guard in Guards)
            {
                Map.RemoveMob(guard);
                guard.HealthManager.OnDead -= Guard_OnDead;
            }
            foreach (var guardTimer in _guardRebirthTimers)
            {
                guardTimer.Timer.Stop();
                guardTimer.Timer.Elapsed -= GuardRebirthTimer_Elapsed;
            }

            lock (_syncObjectGuardTimer)
            {
                _guardRebirthTimers.Clear();
            }
            Guards.Clear();
        }

        #endregion

        #region Dispose

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Obelisk));

            _isDisposed = true;

            ClearGuards();

            ObeliskAI.HealthManager.OnDead -= ObeliskAI_OnDead;
            ObeliskAI = null;

            _rebirthTimer.Elapsed -= ObeliskRebirthTimer_Elapsed;
            _rebirthTimer.Stop();

            Map = null;
        }

        #endregion
    }
}
