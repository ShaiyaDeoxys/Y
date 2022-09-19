using Imgeneus.Database.Entities;
using Imgeneus.Game.Blessing;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Time;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Game.Zone.MapConfig;
using Imgeneus.World.Game.Zone.Portals;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game
{
    /// <summary>
    /// The virtual representation of game world.
    /// </summary>
    public class GameWorld : IGameWorld
    {
        private readonly ILogger<GameWorld> _logger;
        private readonly IMapsLoader _mapsLoader;
        private readonly IMapFactory _mapFactory;
        private readonly ITimeService _timeService;
        private readonly IGuildRankingManager _guildRankingManager;
        private MapDefinitions _mapDefinitions;

        public GameWorld(ILogger<GameWorld> logger, IMapsLoader mapsLoader, IMapFactory mapFactory, ITimeService timeService, IGuildRankingManager guildRankingManager)
        {
            _logger = logger;
            _mapsLoader = mapsLoader;
            _mapFactory = mapFactory;
            _timeService = timeService;
            _guildRankingManager = guildRankingManager;
        }

        #region Init

        private bool _initialized;

        public void Init()
        {
            if (_initialized)
                return;

            InitMaps();
            InitGRB();

            _initialized = true;
        }

        #endregion

        #region Maps

        /// <inheritdoc/>
        public IList<ushort> AvailableMapIds { get; private set; } = new List<ushort>();

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is map id.
        /// </summary>
        public ConcurrentDictionary<ushort, IMap> Maps { get; private set; } = new ConcurrentDictionary<ushort, IMap>();

        /// <summary>
        /// Initializes maps with startup values like mobs, npc, areas, obelisks etc.
        /// </summary>
        private void InitMaps()
        {
            _mapDefinitions = _mapsLoader.LoadMapDefinitions();
            foreach (var mapDefinition in _mapDefinitions.Maps)
            {
                var config = _mapsLoader.LoadMapConfiguration(mapDefinition.Id);
                if (config is null)
                    continue;

                if (mapDefinition.CreateType == CreateType.Default)
                {
                    var map = _mapFactory.CreateMap(mapDefinition.Id, mapDefinition, config, this, _mapsLoader.GetObelisks(mapDefinition.Id), _mapsLoader.GetBosses(mapDefinition.Id));
                    if (Maps.TryAdd(mapDefinition.Id, map))
                        _logger.LogInformation("Map {id} was successfully loaded.", map.Id);
                }

                AvailableMapIds.Add(mapDefinition.Id);
            }
        }

        /// <inheritdoc />
        public bool CanTeleport(Character player, ushort destinationMapId, out PortalTeleportNotAllowedReason reason, bool skipLevelCheck = false)
        {
            reason = PortalTeleportNotAllowedReason.Unknown;

            var destinationMapDef = _mapDefinitions.Maps.FirstOrDefault(d => d.Id == destinationMapId);
            if (destinationMapDef is null)
            {
                _logger.LogWarning("Map {id} is not found in map definitions.", destinationMapId);
                return false;
            }

            if (Maps.ContainsKey(destinationMapId) || player.Map.Id == destinationMapId)
            {
                if (!skipLevelCheck)
                {
                    if (player.LevelProvider.Level < destinationMapDef.MinLevel)
                    {
                        reason = PortalTeleportNotAllowedReason.Unknown;
                        return false;
                    }

                    if (player.LevelProvider.Level > destinationMapDef.MaxLevel)
                    {
                        reason = PortalTeleportNotAllowedReason.Unknown;
                        return false;
                    }
                }

                return true;
            }
            else // Not "usual" map.
            {
                if (destinationMapDef.CreateType == CreateType.Party)
                {
                    if (player.PartyManager.Party is null)
                    {
                        reason = PortalTeleportNotAllowedReason.OnlyForParty;
                        return false;
                    }

                    if (player.PartyManager.Party != null && (player.PartyManager.Party.Members.Count < destinationMapDef.MinMembersCount || (destinationMapDef.MaxMembersCount != 0 && player.PartyManager.Party.Members.Count > destinationMapDef.MaxMembersCount)))
                    {
                        reason = PortalTeleportNotAllowedReason.NotEnoughPartyMembers;
                        return false;
                    }

                    return true;
                }

                if (destinationMapDef.CreateType == CreateType.GuildHouse)
                {
                    if (!player.GuildManager.HasGuild)
                    {
                        reason = PortalTeleportNotAllowedReason.OnlyForGuilds;
                        return false;
                    }

                    if (!player.GuildManager.HasGuildHouse)
                    {
                        reason = PortalTeleportNotAllowedReason.NoGuildHouse;
                        return false;
                    }

                    if (!player.GuildManager.HasTopRank)
                    {
                        reason = PortalTeleportNotAllowedReason.OnlyTop30Guilds;
                        return false;
                    }

                    if (!player.GuildManager.HasPaidGuildHouse)
                    {
                        reason = PortalTeleportNotAllowedReason.FeeNotPaid;
                        return false;
                    }

                    return true;
                }

                if (destinationMapDef.CreateType == CreateType.GRB)
                {
                    if (!player.GuildManager.HasGuild)
                    {
                        reason = PortalTeleportNotAllowedReason.OnlyForGuilds;
                        return false;
                    }

                    if (!destinationMapDef.IsOpen(_timeService.UtcNow))
                    {
                        reason = PortalTeleportNotAllowedReason.NotTimeForRankingBattle;
                        return false;
                    }

                    if (_guildRankingManager.ParticipatedPlayers.Contains(player.Id))
                    {
                        reason = PortalTeleportNotAllowedReason.AlreadyParticipatedInBattle;
                        return false;
                    }

                    return true;
                }

                if (!destinationMapDef.IsOpen(_timeService.UtcNow))
                {
                    reason = PortalTeleportNotAllowedReason.OnlyForPartyAndOnTime;
                    return false;
                }

                return true;
            }
        }

        /// <inheritdoc/>
        public void EnsureMap(DbCharacter dbCharacter, Fraction faction)
        {
            // Map was completely deleted from the server. Fallback to map 0.
            if (!AvailableMapIds.Contains(dbCharacter.Map))
            {
                var coordinates = Maps[0].GetNearestSpawn(0, 0, 0, faction == Fraction.Light ? CountryType.Light : CountryType.Dark);
                dbCharacter.Map = 0;
                dbCharacter.PosX = coordinates.X;
                dbCharacter.PosY = coordinates.Y;
                dbCharacter.PosZ = coordinates.Z;
                return;
            }

            var definition = _mapDefinitions.Maps.First(m => m.Id == dbCharacter.Map);

            // All fine, map is presented on server, character level is ok.
            if (Maps.ContainsKey(dbCharacter.Map) && dbCharacter.Level > definition.MinLevel && dbCharacter.Level <= definition.MaxLevel)
            {
                return;
            }

            if (dbCharacter.Level < definition.MinLevel || dbCharacter.Level > definition.MaxLevel)
            {
                if (faction == Fraction.Light)
                {
                    dbCharacter.Map = definition.LevelOutMapLight.MapId;
                    dbCharacter.PosX = definition.LevelOutMapLight.PosX;
                    dbCharacter.PosY = definition.LevelOutMapLight.PosY;
                    dbCharacter.PosZ = definition.LevelOutMapLight.PosZ;
                    return;
                }

                if (faction == Fraction.Dark)
                {
                    dbCharacter.Map = definition.LevelOutMapDark.MapId;
                    dbCharacter.PosX = definition.LevelOutMapDark.PosX;
                    dbCharacter.PosY = definition.LevelOutMapDark.PosY;
                    dbCharacter.PosZ = definition.LevelOutMapDark.PosZ;
                    return;
                }
            }

            // Map is an instance map. Likely for guild or party. Find out what is the rebirth map.
            if (!Maps.ContainsKey(dbCharacter.Map))
            {
                if (definition.RebirthMap != null) // Rebirth map for both factions set.
                {
                    dbCharacter.Map = definition.RebirthMap.MapId;
                    dbCharacter.PosX = definition.RebirthMap.PosX;
                    dbCharacter.PosY = definition.RebirthMap.PosY;
                    dbCharacter.PosZ = definition.RebirthMap.PosZ;
                    return;
                }

                if (faction == Fraction.Light)
                {
                    dbCharacter.Map = definition.LightRebirthMap.MapId;
                    dbCharacter.PosX = definition.LightRebirthMap.PosX;
                    dbCharacter.PosY = definition.LightRebirthMap.PosY;
                    dbCharacter.PosZ = definition.LightRebirthMap.PosZ;
                    return;
                }

                if (faction == Fraction.Dark)
                {
                    dbCharacter.Map = definition.DarkRebirthMap.MapId;
                    dbCharacter.PosX = definition.DarkRebirthMap.PosX;
                    dbCharacter.PosY = definition.DarkRebirthMap.PosY;
                    dbCharacter.PosZ = definition.DarkRebirthMap.PosZ;
                    return;
                }
            }

            _logger.LogError("Couldn't ensure map {id} for player {characterId}! Check it manually!", dbCharacter.Map, dbCharacter.Id);
        }


        #endregion

        #region Party Maps

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is party id.
        /// </summary>
        public ConcurrentDictionary<Guid, IPartyMap> PartyMaps { get; private set; } = new ConcurrentDictionary<Guid, IPartyMap>();

        private void PartyMap_OnAllMembersLeft(IPartyMap senser)
        {
            senser.OnAllMembersLeft -= PartyMap_OnAllMembersLeft;
            PartyMaps.TryRemove(senser.PartyId, out var removed);
            removed.Dispose();
        }

        #endregion

        #region Guild

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is guild id.
        /// </summary>
        public ConcurrentDictionary<uint, IGuildMap> GuildHouseMaps { get; private set; } = new ConcurrentDictionary<uint, IGuildMap>();

        /// <summary>
        /// Thread-safe dictionary of maps. Where key is guild id.
        /// </summary>
        public ConcurrentDictionary<uint, IGuildMap> GRBMaps { get; private set; } = new ConcurrentDictionary<uint, IGuildMap>();

        /// <summary>
        /// Inits guild ranking battle timers.
        /// </summary>
        private void InitGRB()
        {
            _guildRankingManager.OnStartSoon += GuildRankingManager_OnStartSoon;
            _guildRankingManager.OnStarted += GuildRankingManager_OnStarted;
            _guildRankingManager.On10MinsLeft += GuildRankingManager_On10MinsLeft;
            _guildRankingManager.On1MinLeft += GuildRankingManager_On1MinLeft;
            _guildRankingManager.OnRanksCalculated += GuildRankingManager_OnRanksCalculated;
        }

        private void GuildRankingManager_OnStartSoon()
        {
            foreach (var player in Players.Values.ToList())
                player.SendGRBStartsSoon();
        }

        private void GuildRankingManager_OnStarted()
        {
            foreach (var player in Players.Values.ToList())
                player.SendGRBStarted();
        }

        private void GuildRankingManager_On10MinsLeft()
        {
            foreach (var player in Players.Values.ToList())
                player.SendGRB10MinsLeft();
        }

        private void GuildRankingManager_On1MinLeft()
        {
            foreach (var player in Players.Values.ToList())
                player.SendGRB1MinLeft();
        }

        private void GuildRankingManager_OnRanksCalculated(IEnumerable<(uint guildId, int points, byte rank)> results)
        {
            foreach (var player in Players.Values.ToList())
            {
                player.GuildManager.ReloadGuildRanks(results);
                player.SendGuildRanksCalculated(results);
            }
        }

        #endregion

        #region Players

        /// <inheritdoc />
        public ConcurrentDictionary<uint, Character> Players { get; private set; } = new ConcurrentDictionary<uint, Character>();

        /// <inheritdoc />
        public bool TryLoadPlayer(Character newPlayer)
        {
            var result = Players.TryAdd(newPlayer.Id, newPlayer);

            if (result)
                _logger.LogDebug("Player {id} connected to game world", newPlayer.Id);
            else
                _logger.LogError("Could not load player {id} to game world", newPlayer.Id);

            return result;
        }

        /// <inheritdoc />
        public void LoadPlayerInMap(uint characterId)
        {
            var player = Players[characterId];
            if (Maps.ContainsKey(player.MapProvider.NextMapId))
            {
                Maps[player.MapProvider.NextMapId].LoadPlayer(player);
            }
            else
            {
                var mapDef = _mapDefinitions.Maps.FirstOrDefault(d => d.Id == player.MapProvider.NextMapId);

                // Map is not found.
                if (mapDef is null)
                {
                    _logger.LogWarning("Unknown map {id} for character {characterId}. Fallback to 0 map.", player.MapProvider.NextMapId, player.Id);
                    var town = Maps[0].GetNearestSpawn(player.PosX, player.PosY, player.PosZ, player.CountryProvider.Country);
                    player.TeleportationManager.Teleport(0, town.X, town.Y, town.Z);
                    return;
                }

                if (mapDef.CreateType == CreateType.Party)
                {
                    IPartyMap map;
                    Guid partyId;

                    if (player.PartyManager.Party is null)
                    // This is very uncommon, but if:
                    // * player is an admin he can load into map even without party.
                    // * player entered portal, while being in party, but while he was loading, all party members left.
                    {
                        partyId = player.PartyManager.PreviousPartyId;
                    }
                    else
                    {
                        partyId = player.PartyManager.Party.Id;
                    }

                    PartyMaps.TryGetValue(partyId, out map);
                    if (map is null)
                    {
                        map = _mapFactory.CreatePartyMap(mapDef.Id, mapDef, _mapsLoader.LoadMapConfiguration(mapDef.Id), this, player.PartyManager.Party, _mapsLoader.GetBosses(mapDef.Id));
                        map.OnAllMembersLeft += PartyMap_OnAllMembersLeft;
                        PartyMaps.TryAdd(partyId, map);
                    }

                    map.LoadPlayer(player);
                }

                if (mapDef.CreateType == CreateType.GuildHouse || mapDef.CreateType == CreateType.GRB)
                {
                    uint guildId = 0;
                    if (!player.GuildManager.HasGuild) // probably guild id has changed during loading in portal? Or it's admin without guild tries to load into GBR map.
                    {
                        _logger.LogWarning("Trying to load character {id} without guild id to guild specific map. Fallback to 0.", player.Id);
                    }
                    else
                    {
                        guildId = player.GuildManager.GuildId;
                    }

                    if (mapDef.CreateType == CreateType.GuildHouse)
                    {
                        IGuildMap map;
                        GuildHouseMaps.TryGetValue(guildId, out map);
                        if (map is null)
                        {
                            map = _mapFactory.CreateGuildMap(mapDef.Id, mapDef, _mapsLoader.LoadMapConfiguration(mapDef.Id), this, guildId, _mapsLoader.GetBosses(mapDef.Id));
                            GuildHouseMaps.TryAdd(guildId, map);
                        }

                        map.LoadPlayer(player);
                        return;
                    }

                    if (mapDef.CreateType == CreateType.GRB)
                    {
                        IGuildMap map;
                        GRBMaps.TryGetValue(guildId, out map);
                        if (map is null)
                        {
                            map = _mapFactory.CreateGuildMap(mapDef.Id, mapDef, _mapsLoader.LoadMapConfiguration(mapDef.Id), this, guildId, _mapsLoader.GetBosses(mapDef.Id));
                            GRBMaps.TryAdd(guildId, map);
                        }

                        map.LoadPlayer(player);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void RemovePlayer(uint characterId)
        {
            Character player;
            if (Players.TryRemove(characterId, out player))
            {
                _logger.LogDebug("Player {characterId} left game world", characterId);

                IMap map = null;

                // Try find player's map.
                if (Maps.ContainsKey(player.MapProvider.NextMapId))
                    map = Maps[player.MapProvider.NextMapId];
                else if (player.PartyManager.Party != null && PartyMaps.ContainsKey(player.PartyManager.Party.Id))
                    map = PartyMaps[player.PartyManager.Party.Id];
                else if (PartyMaps.ContainsKey(player.PartyManager.PreviousPartyId))
                    map = PartyMaps[player.PartyManager.PreviousPartyId];
                else if (player.GuildManager.HasGuild && GuildHouseMaps.ContainsKey(player.GuildManager.GuildId))
                    map = GuildHouseMaps[player.GuildManager.GuildId];
                else if (player.GuildManager.HasGuild && GRBMaps.ContainsKey(player.GuildManager.GuildId))
                    map = GRBMaps[player.GuildManager.GuildId];

                if (map is null)
                    _logger.LogError("Couldn't find character's {characterId} map {mapId}.", characterId, player.MapProvider.Map.Id);
                else
                    map.UnloadPlayer(player.Id, exitGame: true);

                player.Dispose();
            }
            else
            {
                // 0 means, that connection with client was lost, when he was in character selection screen.
                if (characterId != 0)
                {
                    _logger.LogError("Couldn't remove player {characterId} from game world", characterId);
                }
            }

        }

        #endregion
    }
}
