using Imgeneus.World.Game.Session;
using System;

namespace Imgeneus.World.Game.Kills
{
    public interface IKillsManager : ISessionedService, IDisposable
    {
        void Init(uint ownerId, ushort kills = 0, ushort deaths = 0, ushort victories = 0, ushort defeats = 0);

        /// <summary>
        /// Event is fired, when number of kills changes.
        /// </summary>
        event Action<uint, ushort> OnKillsChanged;

        /// <summary>
        /// Event is fired, when any of kills, deaths, victories or defeats changes.
        /// </summary>
        event Action<byte, ushort> OnCountChanged;

        ushort Kills { get; set; }
        ushort Deaths { get; set; }
        ushort Victories { get; set; }
        ushort Defeats { get; set; }
    }
}
