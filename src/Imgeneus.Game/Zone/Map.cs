using Imgeneus.Core.Extensions;
using Imgeneus.Database.Entities;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Monster;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Obelisks;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.NpcQuest;
using Parsec.Shaiya.Svmap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Npc = Imgeneus.World.Game.NPCs.Npc;
using Timer = System.Timers.Timer;

namespace Imgeneus.World.Game.Zone
{
    /// <summary>
    /// Zone, where users, mobs, npc are presented.
    /// </summary>
    public class Map : IMap
    {
        #region Constructor

        private readonly MapDefinition _definition;
        private readonly Svmap _config;
        private readonly IEnumerable<ObeliskConfiguration> _obelisksConfig;
        private readonly IEnumerable<BossConfiguration> _bossesConfig;
        private readonly ILogger<Map> _logger;
        private readonly IGamePacketFactory _packetFactory;
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IMobFactory _mobFactory;
        private readonly INpcFactory _npcFactory;
        private readonly IObeliskFactory _obeliskfactory;
        private readonly ITimeService _timeService;

        public IGamePacketFactory PacketFactory { get => _packetFactory; }

        public IGameWorld GameWorld { get; set; }

        /// <summary>
        /// Map id.
        /// </summary>
        public ushort Id { get; private set; }

        /// <summary>
        /// Is map a dungeon?
        /// </summary>
        public bool IsDungeon { get => _definition.MapType == MapType.Dungeon; }

        /// <summary>
        /// Is this map created for party, guild etc. ?
        /// </summary>
        public virtual bool IsInstance { get => false; }

        /// <summary>
        /// How map was created.
        /// </summary>
        public CreateType MapCreationType { get => _definition.CreateType; }

        public static readonly ushort TEST_MAP_ID = 9999;

        public Map(ushort id, MapDefinition definition, Svmap config, IEnumerable<ObeliskConfiguration> obelisks, IEnumerable<BossConfiguration> bosses, ILogger<Map> logger, IGamePacketFactory packetFactory, IGameDefinitionsPreloder definitionsPreloader, IMobFactory mobFactory, INpcFactory npcFactory, IObeliskFactory obeliskFactory, ITimeService timeService)
        {
            Id = id;
            _definition = definition;
            _config = config;
            _obelisksConfig = obelisks;
            _bossesConfig = bosses;
            _logger = logger;
            _packetFactory = packetFactory;
            _definitionsPreloader = definitionsPreloader;
            _mobFactory = mobFactory;
            _npcFactory = npcFactory;
            _obeliskfactory = obeliskFactory;
            _timeService = timeService;

            Init();
        }

        /// <summary>
        /// Inits mobs, npcs, portals etc. based on map configuration.
        /// </summary>
        private void Init()
        {
            CalculateCells(_config.MapSize, _config.CellSize);

            //InitWeather();
            InitPortals();

#if !DEBUG

            InitNPCs();
            InitMobs();
            InitBosses();
            InitObelisks();
            InitOpenCloseTimers();
#endif
        }

        #endregion

        #region Cells

        public int Size { get; private set; }

        /// <summary>
        /// Minimum cell size.
        /// </summary>
        public int MinCellSize { get; private set; }

        /// <summary>
        /// Number of cells rows.
        /// </summary>
        public int Rows { get; private set; }

        /// <summary>
        /// Number of cells columns.
        /// </summary>
        public int Columns { get; private set; }

        public List<MapCell> Cells { get; private set; } = new List<MapCell>();

        /// <summary>
        /// For better performance map sends updates not about the whole map,
        /// but based on cells. Each map is responsible for its cells update.
        /// </summary>
        /// <param name="size">map size</param>
        public void CalculateCells(int size, int cellSize)
        {
            Size = size;
            MinCellSize = cellSize;
            if (MinCellSize == 0)
                MinCellSize = 128;

            var mod = Size / MinCellSize;
            var div = Size % MinCellSize;

            Rows = div == 0 ? mod : mod + 1;
            Columns = Rows;

            for (var i = 0; i < Rows * Columns; i++)
            {
                Cells.Add(new MapCell(i, GetNeighborCellIndexes(i), this));
            }
        }

        /// <summary>
        /// Each map member is assigned cell index as soon as he enters into map or moves.
        /// </summary>
        /// <param name="member">map member</param>
        /// <returns>cell index of this map member</returns>
        public int GetCellIndex(IMapMember member)
        {
            if (member.PosX == 0 || member.PosZ == 0)
                return 0;

            int row = ((int)Math.Round(member.PosX, 0)) / MinCellSize;
            int column = ((int)Math.Round(member.PosZ, 0)) / MinCellSize;

            return row + (column * Rows);
        }

        /// <summary>
        /// Gets indexes of neighbor cells.
        /// </summary>
        /// <param name="cellIndex">main cell index</param>
        /// <returns>list of neighbor cell indexes</returns>
        public IEnumerable<int> GetNeighborCellIndexes(int cellIndex)
        {
            var neighbors = new List<int>();
            var myRow = cellIndex % Rows;
            var myColumn = (cellIndex - myRow) / Columns;

            var left = cellIndex - 1;
            if (left >= 0 && (left / Columns) == myColumn)
                neighbors.Add(left);

            var right = cellIndex + 1;
            if (right < Rows * Columns && (right / Columns) == myColumn)
                neighbors.Add(right);

            var top = cellIndex - Rows;
            if (top >= 0)
                neighbors.Add(top);

            var bottom = cellIndex + Rows;
            if (bottom < Rows * Columns)
                neighbors.Add(bottom);

            var column = 0;

            var topleft = cellIndex - Rows - 1;
            column = (topleft - (topleft % Rows)) / Columns;
            if (topleft >= 0 && topleft < Rows * Columns && column == myColumn - 1)
                neighbors.Add(topleft);

            var topright = cellIndex - Rows + 1;
            column = (topright - (topright % Rows)) / Columns;
            if (topright >= 0 && topright < Rows * Columns && column == myColumn - 1)
                neighbors.Add(topright);

            var bottomleft = cellIndex + Rows - 1;
            column = (bottomleft - (bottomleft % Rows)) / Columns;
            if (bottomleft >= 0 && bottomleft < Rows * Columns && column == myColumn + 1)
                neighbors.Add(bottomleft);

            var bottomright = cellIndex + Rows + 1;
            column = (bottomright - (bottomright % Rows)) / Columns;
            if (bottomright >= 0 && bottomright < Rows * Columns && column == myColumn + 1)
                neighbors.Add(bottomright);

            return neighbors.OrderBy(i => i);
        }

        #endregion

        #region Players

        /// <summary>
        /// Thread-safe dictionary of connected players. Key is character id, value is character.
        /// </summary>
        public ConcurrentDictionary<uint, Character> Players { get; init; } = new ConcurrentDictionary<uint, Character>();

        /// <summary>
        /// Tries to get player from map.
        /// </summary>
        /// <param name="playerId">id of player, that you are trying to get.</param>
        /// <returns>either player or null if player is not presented</returns>
        public Character GetPlayer(uint playerId)
        {
            Players.TryGetValue(playerId, out var player);
            return player;
        }

        /// <summary>
        /// Loads player into map.
        /// </summary>
        /// <param name="character">player, that we need to load</param>
        /// <returns>returns true if we could load player to map, otherwise false</returns>
        public virtual bool LoadPlayer(Character character)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            var success = Players.TryAdd(character.Id, character);

            if (success)
            {
                character.Map = this;
                Cells[GetCellIndex(character)].AddPlayer(character);

                character.MovementManager.OnMove += Character_OnMove;
                _logger.LogDebug($"Player {character.Id} connected to map {Id}, cell index {character.CellId}.");
            }
            else
            {
                _logger.LogError($"Failed to load player {character.Id} connected to map {Id}");
            }

            return success;
        }

        /// <summary>
        /// Unloads player from map.
        /// </summary>
        /// <param name="characterId">player, that we need to unload</param>
        /// <param name="exitGame">if case player exits game, he/she should be teleported to rebirth map</param>
        /// <returns>returns true if we could unload player to map, otherwise false</returns>
        public virtual bool UnloadPlayer(uint characterId, bool exitGame = false)
        {
            if (!Players.ContainsKey(characterId))
                return false;

            var character = Players[characterId];

            character.VehicleManager.RemoveVehicle();

            if (exitGame)
            {
                var rebirthMap = GetRebirthMap(character);
                if (rebirthMap.MapId != Id)
                {
                    character.MapProvider.NextMapId = rebirthMap.MapId;
                    character.MovementManager.PosX = rebirthMap.X;
                    character.MovementManager.PosY = rebirthMap.Y;
                    character.MovementManager.PosZ = rebirthMap.Z;
                }

                if (character.HealthManager.IsDead)
                    character.HealthManager.Rebirth();
            }

            Cells[character.CellId].RemovePlayer(character, true);
            UnregisterSearchForParty(character);
            character.MovementManager.OnMove -= Character_OnMove;
            character.OldCellId = -1;
            character.CellId = -1;

            _logger.LogDebug($"Player {character.Id} left map {Id}");

            return Players.TryRemove(characterId, out var _);
        }

        /// <summary>
        /// When player's position changes, we are checking if player's map cell should be changed.
        /// </summary>
        private void Character_OnMove(uint senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            var sender = Players[senderId];
            var newCellId = GetCellIndex(sender);
            var oldCellId = sender.CellId;
            if (oldCellId == newCellId) // All is fine, character is in the right cell
                return;

            if (newCellId >= Cells.Count)
                newCellId = Cells.Count - 1;

            // Need to calculate new cell...
#if DEBUG
            _logger.LogDebug("Character {characterId} change map cell from {oldCellId} to {newCellId}.", sender.Id, oldCellId, newCellId);
#endif
            Cells[oldCellId].RemovePlayer(sender, false);
            Cells[newCellId].AddPlayer(sender);
        }

        /// <inheritdoc/>
        public (float X, float Y, float Z) GetNearestSpawn(float currentX, float currentY, float currentZ, CountryType country)
        {
            Spawn nearestSpawn = null;
            foreach (var spawn in _config.Spawns)
            {
                if ((int)spawn.Faction == 1 && country == CountryType.Light || (int)spawn.Faction == 2 && country == CountryType.Dark)
                {
                    if (nearestSpawn is null)
                    {
                        nearestSpawn = spawn;
                        continue;
                    }

                    if (MathExtensions.Distance(currentX, spawn.Area.LowerLimit.X, currentZ, spawn.Area.LowerLimit.Z) < MathExtensions.Distance(currentX, nearestSpawn.Area.LowerLimit.X, currentZ, nearestSpawn.Area.LowerLimit.Z))
                        nearestSpawn = spawn;
                }
            }

            if (nearestSpawn is null)
            {
                _logger.LogError("Spawn for map {id} is not found. Rebirth at the same coordinate.", Id);
                return (currentX, currentY, currentZ);
            }

            var random = new Random();
            var x = random.NextFloat(nearestSpawn.Area.LowerLimit.X, nearestSpawn.Area.UpperLimit.X);
            var y = random.NextFloat(nearestSpawn.Area.LowerLimit.Y, nearestSpawn.Area.UpperLimit.Y);
            var z = random.NextFloat(nearestSpawn.Area.LowerLimit.Z, nearestSpawn.Area.UpperLimit.Z);
            return (x, y, z);
        }

        /// <inheritdoc/>
        public (ushort MapId, float X, float Y, float Z) GetRebirthMap(Character player)
        {
            if (_definition.RebirthMap != null)
                return (_definition.RebirthMap.MapId, _definition.RebirthMap.PosX, _definition.RebirthMap.PosY, _definition.RebirthMap.PosZ);

            if (_definition.LightRebirthMap != null && player.CountryProvider.Country == CountryType.Light)
                return (_definition.LightRebirthMap.MapId, _definition.LightRebirthMap.PosX, _definition.LightRebirthMap.PosY, _definition.LightRebirthMap.PosZ);

            if (_definition.DarkRebirthMap != null && player.CountryProvider.Country == CountryType.Dark)
                return (_definition.DarkRebirthMap.MapId, _definition.DarkRebirthMap.PosX, _definition.DarkRebirthMap.PosY, _definition.DarkRebirthMap.PosZ);

            // There is no rebirth map, use the nearest spawn.
            var spawn = GetNearestSpawn(player.PosX, player.PosY, player.PosZ, player.CountryProvider.Country);
            return (Id, spawn.X, spawn.Y, spawn.Z);
        }

        #region Party search

        /// <summary>
        /// Collection of players, that are looking for party.
        /// </summary>
        public List<Character> PartySearchers { get; private set; } = new List<Character>();

        private object _partySearchSync = new object();

        /// <summary>
        /// Registers player is party searchers.
        /// </summary>
        public void RegisterSearchForParty(Character character)
        {
            lock (_partySearchSync)
            {
                if (!PartySearchers.Contains(character))
                    PartySearchers.Add(character);
            }
        }

        /// <summary>
        /// Removes player from party searchers.
        /// </summary>
        public void UnregisterSearchForParty(Character character)
        {
            lock (_partySearchSync)
            {
                PartySearchers.Remove(character);
            }
        }

        #endregion

        #endregion

        #region Mobs

        /// <summary>
        /// Mob Id per map.
        /// </summary>
        private uint _currentGlobalMobId = 1;
        private readonly object _currentGlobalMobIdMutex = new object();

        /// <summary>
        /// Each entity in game has its' own id.
        /// Call this method, when you need to get new id.
        /// </summary>
        public uint GenerateId()
        {
            lock (_currentGlobalMobIdMutex)
            {
                _currentGlobalMobId++;
            }
            return _currentGlobalMobId;
        }

        /// <summary>
        /// Tries to add mob to map.
        /// </summary>
        public void AddMob(Mob mob)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            mob.Init(GenerateId());
            mob.Map = this;
            Cells[GetCellIndex(mob)].AddMob(mob);
            mob.AIManager.Start();

            if (mob.ShouldRebirth)
                mob.TimeToRebirth += RebirthMob;

#if DEBUG
            _logger.LogDebug("Mob {mobId} with global id {id} entered map {mapId}", mob.MobId, mob.Id, Id);
#endif
        }

        /// <summary>
        /// Called, when mob respawns.
        /// </summary>
        /// <param name="sender">respawned mob</param>
        public void RebirthMob(Mob sender)
        {
            sender.TimeToRebirth -= RebirthMob;

            if (_isDisposed) // Map already disposed.
                return;

            // Create mob clone, because we can not reuse the same id.
            var mob = sender.Clone();

            // TODO: generate rebirth coordinates based on the spawn area.
            mob.MovementManager.PosX = sender.PosX;
            mob.MovementManager.PosY = sender.PosY;
            mob.MovementManager.PosZ = sender.PosZ;

            mob.HealthManager.Rebirth();

            AddMob(mob);
        }

        /// <summary>
        /// Tries to remove mob from map.
        /// </summary>
        /// <param name="mob"></param>
        public void RemoveMob(Mob mob)
        {
            mob.AIManager.Stop();
            Cells[mob.CellId].RemoveMob(mob);
        }

        /// <summary>
        /// Tries to get mob from map.
        /// </summary>
        /// <param name="cellId">map cell index</param>
        /// <param name="mobId">id of mob, that you are trying to get.</param>
        /// <returns>either mob or null if mob is not presented</returns>
        public Mob GetMob(int cellId, uint mobId)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            return Cells[cellId].GetMob(mobId, true);
        }

        private void InitMobs()
        {
            int finalMobsCount = 0;
            foreach (var mobArea in _config.MonsterAreas)
            {
                foreach (var mobConf in mobArea.Monsters)
                {
                    if (_definitionsPreloader.Mobs.ContainsKey((ushort)mobConf.MobId))
                    {
                        for (var i = 0; i < mobConf.Count; i++)
                        {
                            var mob = _mobFactory.CreateMob((ushort)mobConf.MobId,
                                                            true,
                                                            new MoveArea(mobArea.Area.LowerLimit.X, mobArea.Area.UpperLimit.X, mobArea.Area.LowerLimit.Y, mobArea.Area.UpperLimit.Y, mobArea.Area.LowerLimit.Z, mobArea.Area.UpperLimit.Z));
                            AddMob(mob);
                            finalMobsCount++;
                        }
                    }
                }
            }

            _logger.LogInformation("Map {id} created {count} mob areas.", Id, _config.MonsterAreas.Count);
            _logger.LogInformation("Map {id} created {finalMobsCount} mobs.", Id, finalMobsCount);
        }

        private void InitBosses()
        {
            int finalMobsCount = 0;
            foreach (var mobConfig in _bossesConfig)
            {
                if (_definitionsPreloader.Mobs.ContainsKey(mobConfig.MobId))
                {
                    var mob = _mobFactory.CreateMob(mobConfig.MobId,
                                                    true,
                                                    new MoveArea(mobConfig.X, mobConfig.X, mobConfig.Y, mobConfig.Y, mobConfig.Z, mobConfig.Z));

                    mob.RespawnTimeInMilliseconds = mobConfig.RespawnTimeInSeconds * 1000;

                    if (mobConfig.Portal != 0)
                    {
                        mob.HealthManager.OnDead += (uint senderId, IKiller killer) =>
                        {
                            var portal = Portals.FirstOrDefault(x => x.PortalId == mobConfig.Portal);
                            if (portal is null)
                                return;

                            portal.IsOpen = true;
                        };

                        mob.HealthManager.OnRebirthed += (uint senderId) =>
                        {
                            var portal = Portals.FirstOrDefault(x => x.PortalId == mobConfig.Portal);
                            if (portal is null)
                                return;

                            portal.IsOpen = false;
                        };
                    }

                    AddMob(mob);
                    finalMobsCount++;
                }
            }

            _logger.LogInformation("Map {id} created {finalMobsCount} bosses.", Id, finalMobsCount);
        }

        #endregion

        #region Items

        private object _syncAddRemoveItem = new();

        /// <summary>
        /// Adds item on map.
        /// </summary>
        /// <param name="item">new added item</param>
        public void AddItem(MapItem item)
        {
            lock (_syncAddRemoveItem)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(Map));

                item.Id = GenerateId();
                Cells[GetCellIndex(item)].AddItem(item);
                item.OnRemove += Item_OnRemove;
            }
        }

        /// <summary>
        /// Tries to get item from map.
        /// </summary>
        /// <returns>if item is null, means that item doesn't belong to player yet</returns>
        public (MapItem Item, bool notOnMap) GetItem(uint itemId, Character requester)
        {
            lock (_syncAddRemoveItem)
            {
                return Cells[requester.CellId].GetItem(itemId, requester, true);
            }
        }

        /// <summary>
        /// Removes item from map.
        /// </summary>
        public void RemoveItem(int cellId, uint itemId)
        {
            lock (_syncAddRemoveItem)
            {
                var item = Cells[cellId].RemoveItem(itemId, true);
                if (item != null)
                {
                    item.OnRemove -= Item_OnRemove;
                    item.Dispose();
                }
            }
        }

        private void Item_OnRemove(MapItem item)
        {
            RemoveItem(item.CellId, item.Id);
        }

        #endregion

        #region NPC

        /// <summary>
        /// Adds npc to the map.
        /// </summary>
        /// <param name="cellIndex">cell index</param>
        /// <param name="npc">new npc</param>
        public void AddNPC(int cellIndex, Npc npc)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            Cells[cellIndex].AddNPC(npc);

            if (npc is GuardNpc guard)
                guard.AIManager.Start();
        }

        /// <summary>
        /// Removes NPC from the map.
        /// </summary>
        public void RemoveNPC(int cellIndex, NpcType type, ushort typeId, byte count)
        {
            Cells[cellIndex].RemoveNPC(type, typeId, count);
        }

        /// <summary>
        /// Gets npc by its' id.
        /// </summary>
        public NPCs.Npc GetNPC(int cellIndex, uint id)
        {
            return Cells[cellIndex].GetNPC(id, true);
        }

        private void InitNPCs()
        {
            int finalNPCsCount = 0;
            foreach (var conf in _config.Npcs)
            {
                var moveCoordinates = conf.Locations.Select(c => (c.Position.X, c.Position.Y, c.Position.Z, Convert.ToUInt16(c.Orientation))).ToList();
                var npc = _npcFactory.CreateNpc(((NpcType)conf.Type, (short)conf.NpcId), moveCoordinates, this);
                if (npc is null)
                    continue;

                npc.Init(GenerateId());
                npc.Map = this;

                var cellIndex = GetCellIndex(npc);
                AddNPC(cellIndex, npc);
                finalNPCsCount++;
            }

            _logger.LogInformation($"Map {Id} created {finalNPCsCount} NPCs.");
        }

        #endregion

        #region Weather

        private readonly System.Timers.Timer _weatherTimer = new System.Timers.Timer();

        private WeatherState _weatherState = WeatherState.None;

        public WeatherState WeatherState
        {
            get
            {
                if (IsDungeon)
                    return WeatherState.None;

                return _weatherState;
            }
        }

        public WeatherPower WeatherPower
        {
            get
            {
                return _definition.WeatherPower;
            }
        }

        private void InitWeather()
        {
            if (IsDungeon)
                return;

            _weatherTimer.AutoReset = false;

            if (_definition.NoneWeatherDuration == 0)
                _weatherState = _definition.WeatherState;

            if (_definition.WeatherDuration > 0 && _definition.NoneWeatherDuration != 0 && _definition.WeatherState != WeatherState.None)
            {
                _weatherTimer.Elapsed += WeatherTimer_Elapsed;
                StartWeatherTimer();
            }
        }

        private void WeatherTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            StartWeatherTimer();
        }

        /// <summary>
        /// Starts new weather.
        /// </summary>
        private void StartWeatherTimer()
        {
            if (_weatherState == WeatherState.None)
            {
                // Start raining/snowing.
                _weatherTimer.Interval = _definition.WeatherDuration * 1000;
                _weatherState = _definition.WeatherState;
            }
            else
            {
                // Start none(good) weather.
                _weatherTimer.Interval = _definition.NoneWeatherDuration * 1000;
                _weatherState = WeatherState.None;
            }

            _logger.LogDebug($"Map {Id} changed weather to {_weatherState}.");

            foreach (var player in Players.Values)
                PacketFactory.SendWeather(player.GameSession.Client, this);

            _weatherTimer.Start();
        }

        #endregion

        #region Obelisks

        /// <summary>
        /// Obelisks on this map.
        /// </summary>
        public ConcurrentDictionary<uint, Obelisk> Obelisks = new ConcurrentDictionary<uint, Obelisk>();

        /// <summary>
        /// Creates obelisks.
        /// </summary>
        private void InitObelisks()
        {
            foreach (var obeliskConfig in _obelisksConfig)
            {
                var obelisk = _obeliskfactory.CreateObelisk(obeliskConfig, this);
                Obelisks.TryAdd(obelisk.Id, obelisk);
                obelisk.OnObeliskBroken += Obelisk_OnObeliskBroken;
            }

            _logger.LogInformation("Map {id} created {count} obelisks.", Id, Obelisks.Count);
        }

        private void Obelisk_OnObeliskBroken(Obelisk obelisk)
        {
            foreach (var character in Players.Values)
                character.SendObeliskBroken(obelisk);
        }

        #endregion

        #region Portals

        public IList<Portals.Portal> Portals { get; private set; } = new List<Portals.Portal>();

        /// <summary>
        /// Creates portals to another maps.
        /// </summary>
        private void InitPortals()
        {
            foreach (var portalConfig in _config.Portals)
            {
                Portals.Add(new Portals.Portal(portalConfig));
            }

            _logger.LogInformation($"Map {Id} created {Portals.Count} portals.");
        }

        #endregion

        #region Open/Closed

        private Timer _openTimer = new Timer();

        private Timer _closeTimer = new Timer();

        /// <inheritdoc/>
        public event Action<IMap> OnOpen;

        /// <inheritdoc/>
        public event Action<IMap> OnClose;

        /// <summary>
        /// Generates special timers for maps, that must be opened/closed in specific time range.
        /// </summary>
        private void InitOpenCloseTimers()
        {
            if (string.IsNullOrEmpty(_definition.OpenTime) || string.IsNullOrEmpty(_definition.CloseTime))
                return;

            _openTimer.Interval = _definition.NextOpenDate(_timeService.UtcNow).Subtract(_timeService.UtcNow).TotalMilliseconds;
            _openTimer.Start();
            _openTimer.AutoReset = false;
            _openTimer.Elapsed += OpenTimer_Elapsed;

            _closeTimer.Interval = _definition.NextCloseDate(_timeService.UtcNow).Subtract(_timeService.UtcNow).TotalMilliseconds;
            _closeTimer.Start();
            _closeTimer.AutoReset = false;
            _closeTimer.Elapsed += CloseTimer_Elapsed;
        }

        private void OpenTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnOpen?.Invoke(this);
        }

        private void CloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnClose?.Invoke(this);

            foreach (var player in Players.Values.ToList())
            {
                var map = GetRebirthMap(player);
                player.TeleportationManager.Teleport(map.MapId, map.X, map.Y, map.Z);
            }
        }

        #endregion

        #region Dispose

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!Players.IsEmpty)
                throw new Exception("Cannot dispose map, until all players have left this map!");

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            _isDisposed = true;

            foreach (var cell in Cells)
            {
                var mobs = cell.GetAllMobs(false);
                var items = cell.GetAllItems(false);

                foreach (var m in mobs)
                    RemoveMob(m);

                foreach (var itm in items)
                    RemoveItem(itm.CellId, itm.Id);

                cell.Dispose();
            }

            foreach (var obelisk in Obelisks.Values)
            {
                obelisk.OnObeliskBroken -= Obelisk_OnObeliskBroken;
                obelisk.Dispose();
            }

            Cells.Clear();
            Portals.Clear();
            Obelisks.Clear();
            PartySearchers.Clear();

            _weatherTimer.Stop();
            _weatherTimer.Elapsed -= WeatherTimer_Elapsed;

            _openTimer.Stop();
            _openTimer.Elapsed -= OpenTimer_Elapsed;

            _closeTimer.Stop();
            _closeTimer.Elapsed -= CloseTimer_Elapsed;
        }

        #endregion
    }
}
