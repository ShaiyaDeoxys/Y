using Imgeneus.Database.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.Database.Preload
{
    /// <inheritdoc />
    public class DatabasePreloader : IDatabasePreloader
    {
        private readonly ILogger<DatabasePreloader> _logger;
        private readonly IDatabase _database;


        /// <inheritdoc />
        public Dictionary<(Mode Mode, ushort Level), DbLevel> Levels { get; private set; } = new Dictionary<(Mode Mode, ushort Level), DbLevel>();

        public DatabasePreloader(ILogger<DatabasePreloader> logger, IDatabase database)
        {
            _logger = logger;
            _database = database;

            Preload();
        }

        /// <summary>
        /// Preloads all needed game definitions from database.
        /// </summary>
        private void Preload()
        {
            try
            {
                PreloadLevels(_database);

                _logger.LogInformation("Database was successfully preloaded.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during preloading database: {ex.Message}");
            }

        }

        /// <summary>
        /// Preloads all available levels/experience from database.
        /// </summary>
        private void PreloadLevels(IDatabase database)
        {
            var levels = database.Levels;
            foreach (var level in levels)
            {
                Levels.Add((level.Mode, level.Level), level);
            }
        }
    }
}
