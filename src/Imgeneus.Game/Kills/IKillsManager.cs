using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Kills
{
    public interface IKillsManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId, uint kills = 0, uint deaths = 0, uint victories = 0, uint defeats = 0, byte killLevel = 1, byte deathLevel = 1);

        /// <summary>
        /// Event is fired, when number of kills changes.
        /// </summary>
        event Action<uint, uint> OnKillsChanged;

        /// <summary>
        /// Event is fired, when any of kills, deaths, victories or defeats changes.
        /// </summary>
        event Action<byte, uint> OnCountChanged;

        uint Kills { get; set; }
        uint Deaths { get; set; }
        uint Victories { get; set; }
        uint Defeats { get; set; }

        /// <summary>
        /// PvP reward level for killed enemies.
        /// </summary>
        byte KillLevel { get; set; }

        /// <summary>
        /// PvP reward level for deathes.
        /// </summary>
        byte DeathLevel { get; set; }

        /// <summary>
        /// Tries to get stats point for kills.
        /// </summary>
        (bool Ok, ushort Stats) TryGetKillsReward();

        /// <summary>
        /// Tries to get money for deaths.
        /// </summary>
        /// <returns></returns>
        (bool Ok, uint Money) TryGetDeathsReward();
    }
}
