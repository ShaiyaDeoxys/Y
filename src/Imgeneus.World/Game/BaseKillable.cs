using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Untouchable;
using Imgeneus.World.Game.Zone;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// Abstract entity, that can be killed. Implements common features for killable object.
    /// </summary>
    public abstract class BaseKillable : IKillable, IMapMember
    {
        protected readonly IDatabasePreloader _databasePreloader;
        public ICountryProvider CountryProvider { get; private set; }
        public IStatsManager StatsManager { get; private set; }
        public IHealthManager HealthManager { get; private set; }
        public ILevelProvider LevelProvider { get; private set; }
        public IBuffsManager BuffsManager { get; private set; }
        public IElementProvider ElementProvider { get; private set; }
        public IMovementManager MovementManager { get; private set; }
        public IUntouchableManager UntouchableManager { get; private set; }
        public IMapProvider MapProvider { get; private set; }

        public BaseKillable(IDatabasePreloader databasePreloader, ICountryProvider countryProvider, IStatsManager statsManager, IHealthManager healthManager, ILevelProvider levelProvider, IBuffsManager buffsManager, IElementProvider elementProvider, IMovementManager movementManager, IUntouchableManager untouchableManager, IMapProvider mapProvider)
        {
            _databasePreloader = databasePreloader;
            CountryProvider = countryProvider;
            StatsManager = statsManager;
            HealthManager = healthManager;
            LevelProvider = levelProvider;
            BuffsManager = buffsManager;
            ElementProvider = elementProvider;
            MovementManager = movementManager;
            UntouchableManager = untouchableManager;
            MapProvider = mapProvider;
        }

        private int _id;

        /// <inheritdoc />
        public int Id
        {
            get => _id;
            set
            {
                if (_id == 0)
                {
                    _id = value;
                }
                else
                {
                    throw new ArgumentException("Id can not be set twice.");
                }
            }
        }

        #region Map

        public Map Map { get => MapProvider.Map; set => MapProvider.Map = value; }

        public int CellId { get; set; } = -1;

        public int OldCellId { get; set; } = -1;

        #endregion

        #region Death

        /// <summary>
        /// Add items on map.
        /// </summary>
        private void AddItemsDropOnMap(IList<Item> dropItems, Character owner)
        {
            byte i = 0;
            foreach (var itm in dropItems)
            {
                MapProvider.Map.AddItem(new MapItem(itm, owner, PosX + i, PosY, PosZ));
                i++;
            }
        }

        /// <summary>
        /// Generates drop for killer.
        /// </summary>
        protected abstract IList<Item> GenerateDrop(IKiller killer);

        #endregion

        #region Position

        public float PosX { get => MovementManager.PosX; }

        public float PosY { get => MovementManager.PosY; }

        public float PosZ { get => MovementManager.PosZ; }

        public ushort Angle { get => MovementManager.Angle; }

        #endregion

        #region Untouchable

        public bool IsUntouchable { get => UntouchableManager.IsUntouchable; }

        #endregion

        #region Absorption

        public ushort Absorption { get => StatsManager.Absorption; }

        #endregion
    }
}
