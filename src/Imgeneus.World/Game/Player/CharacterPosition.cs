using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Duel;
using System;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        /// <summary>
        /// Updates player position. Saves change to database if needed.
        /// </summary>
        /// <param name="x">new x</param>
        /// <param name="y">new y</param>
        /// <param name="z">new z</param>
        /// <param name="saveChangesToDB">set it to true, if this change should be saved to database</param>
        private void UpdatePosition(float x, float y, float z, ushort angle, bool saveChangesToDB)
        {
            
        }
    }
}
