using Imgeneus.Core.Helpers;
using Imgeneus.Game.Monster;
using Imgeneus.World.Game.Zone.Obelisks;
using Microsoft.Extensions.Logging;
using Parsec;
using Parsec.Shaiya.Svmap;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Imgeneus.World.Game.Zone.MapConfig
{
    public class MapsLoader : IMapsLoader
    {
        private readonly ILogger<MapsLoader> _logger;

        public MapsLoader(ILogger<MapsLoader> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Relative path to configs folder.
        /// </summary>
        private const string ConfigsFolder = "config/maps/";

        /// <summary>
        /// File, that contains definitions for all maps, that must be loaded.
        /// </summary>
        private const string InitFile = "MapInit.json";

        /// <summary>
        /// File, that contains definitions for all obelisks, that must be loaded.
        /// </summary>
        private const string ObeliskInitFile = "ObeliskInit.json";

        /// <summary>
        /// File, that contains definitions for all bosses, that must be loaded.
        /// </summary>
        private const string BossInitFile = "BossMobInit.json";

        public MapDefinitions LoadMapDefinitions()
        {
            var initFilePath = Path.Combine(ConfigsFolder, InitFile);
            if (!File.Exists(initFilePath))
            {
                _logger.LogError("No map definition is found.");
                return new MapDefinitions();
            }

            return ConfigurationHelper.Load<MapDefinitions>(initFilePath); ;
        }

        #region Map configs

        private readonly Dictionary<ushort, Svmap> _loadedConfigs = new Dictionary<ushort, Svmap>();

        public Svmap LoadMapConfiguration(ushort mapId)
        {
            if (_loadedConfigs.ContainsKey(mapId))
            {
                return _loadedConfigs[mapId];
            }
            else
            {
                var mapFile = Path.Combine(ConfigsFolder, $"{mapId}.svmap");
                if (!File.Exists(mapFile))
                {
                    _logger.LogError($"Configuration for map {mapId} is not found.");
                    return null;
                }

                var config = Reader.ReadFromFile<Svmap>(mapFile); ;
                _loadedConfigs.Add(mapId, config);
                return config;
            }
        }

        #endregion

        #region Obelisks

        private MapObeliskConfigurations _obelisksConfig;

        public IEnumerable<ObeliskConfiguration> GetObelisks(ushort mapId)
        {
            if (_obelisksConfig == null)
            {
                var obelisksFile = Path.Combine(ConfigsFolder, ObeliskInitFile);
                if (!File.Exists(obelisksFile))
                {
                    _logger.LogError($"Obelisks init file is not found.");
                    return new List<ObeliskConfiguration>();
                }

                _obelisksConfig = ConfigurationHelper.Load<MapObeliskConfigurations>(obelisksFile);
            }

            var mapObelisks = _obelisksConfig.Maps.FirstOrDefault(m => m.MapId == mapId);
            if (mapObelisks == null)
                return new List<ObeliskConfiguration>();
            else
                return mapObelisks.Obelisks;
        }

        #endregion

        #region Bosses

        private MapBossConfigurations _bossesConfig;

        public IEnumerable<BossConfiguration> GetBosses(ushort mapId)
        {
            if (_bossesConfig == null)
            {
                var bossesFile = Path.Combine(ConfigsFolder, BossInitFile);
                if (!File.Exists(bossesFile))
                {
                    _logger.LogError($"Bosses init file is not found.");
                    return new List<BossConfiguration>();
                }

                _bossesConfig = ConfigurationHelper.Load<MapBossConfigurations>(bossesFile);
            }

            var mapBosses = _bossesConfig.Maps.FirstOrDefault(m => m.MapId == mapId);
            if (mapBosses == null)
                return new List<BossConfiguration>();
            else
                return mapBosses.MobBosses;
        }

        #endregion
    }
}
