using Imgeneus.World.Game.Player;
using System;
using System.Collections.Generic;
using Imgeneus.World.Game.Zone.Portals;
using Imgeneus.World.Game.Country;
using System.Collections.Concurrent;

namespace Imgeneus.World.Game.Zone
{
    public interface IMap : IDisposable
    {
        /// <summary>
        /// Map must have unique id.
        /// </summary>
        ushort Id { get; }

        /// <summary>
        /// Loads player into the map.
        /// </summary>
        bool LoadPlayer(Character player);

        /// <summary>
        /// Removes player from the map.
        /// </summary>
        bool UnloadPlayer(uint playerId, bool exitGame = false);

        /// <summary>
        /// Map players.
        /// </summary>
        ConcurrentDictionary<uint, Character> Players { get; }

        /// <summary>
        /// Map portals.
        /// </summary>
        IList<Portal> Portals { get; }

        /// <summary>
        /// Finds the nearest spawn for the player.
        /// </summary>
        /// <param name="currentX">current player x coordinate</param>
        /// <param name="currentY">current player y coordinate</param>
        /// <param name="currentZ">current player z coordinate</param>
        /// <param name="fraction">player's faction</param>
        /// <returns>coordinate, where player should spawn</returns>
        (float X, float Y, float Z) GetNearestSpawn(float currentX, float currentY, float currentZ, CountryType fraction);

        /// <summary>
        ///Gets map, where the character must appear after death or disconnect.
        /// </summary>
        /// <returns>map id and coordinate, where player should spawn</returns>
        (ushort MapId, float X, float Y, float Z) GetRebirthMap(Character player);

        /// <summary>
        /// Is this map created for party, guild etc. ?
        /// </summary>
        bool IsInstance { get; }

        /// <summary>
        /// Fires event, when map is open.
        /// </summary>
        event Action<IMap> OnOpen;

        /// <summary>
        /// Fires event, when map is closed.
        /// </summary>
        event Action<IMap> OnClose;

        /// <summary>
        /// Game world.
        /// </summary>
        IGameWorld GameWorld { get; set; }

        /// <summary>
        /// Map size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// For better performance updates are sent through map cells.
        /// </summary>
        List<MapCell> Cells { get; }
    }
}
