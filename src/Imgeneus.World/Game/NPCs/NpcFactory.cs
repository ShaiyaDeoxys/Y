using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Imgeneus.World.Game.NPCs
{
    public class NpcFactory : INpcFactory
    {
        private readonly ILogger<Npc> _logger;
        private readonly IGameDefinitionsPreloder _preloader;

        public NpcFactory(ILogger<Npc> logger, IGameDefinitionsPreloder preloader)
        {
            _logger = logger;
            _preloader = preloader;
        }

        public Npc CreateNpc((byte Type, short TypeId) id, List<(float X, float Y, float Z, ushort Angle)> moveCoordinates, Map map)
        {
            if (_preloader.NPCs.TryGetValue((id.Type, id.TypeId), out var dbNpc))
            {
                return new Npc(moveCoordinates, map, _logger, dbNpc);
            }
            else
            {
                _logger.LogWarning($"Unknown npc type {id.Type} and type id {id.TypeId}");
                return null;
            }
        }
    }
}
