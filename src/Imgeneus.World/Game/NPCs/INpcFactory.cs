using Imgeneus.World.Game.Zone;
using Parsec.Shaiya.NpcQuest;
using System.Collections.Generic;

namespace Imgeneus.World.Game.NPCs
{
    public interface INpcFactory
    {
        /// <summary>
        /// Creates npc instance.
        /// </summary>
        /// <param name="id">npc type and type id</param>
        /// <param name="moveCoordinates">npc move coordinates</param>
        /// <param name="map">npc's map</param>
        /// <returns>npc instance</returns>
        public Npc CreateNpc((NpcType Type, short TypeId) id, List<(float X, float Y, float Z, ushort Angle)> moveCoordinates, Map map);
    }
}
