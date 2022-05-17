using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Common;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.NPCs
{
    public class NpcFactory : INpcFactory
    {
        private readonly ILogger<Npc> _logger;
        private readonly IGameDefinitionsPreloder _preloader;
        private readonly IServiceProvider _serviceProvider;

        public NpcFactory(ILogger<Npc> logger, IGameDefinitionsPreloder preloader, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _preloader = preloader;
            _serviceProvider = serviceProvider;
        }

        public Npc CreateNpc((NpcType Type, short TypeId) id, List<(float X, float Y, float Z, ushort Angle)> moveCoordinates, Map map)
        {
            if (_preloader.NPCs.TryGetValue((id.Type, id.TypeId), out var dbNpc))
            {
                var scope = _serviceProvider.CreateScope();
                Npc npc;
                if (id.Type == NpcType.Guard)
                {
                    npc = new GuardNpc(_logger,
                                       dbNpc,
                                       moveCoordinates,
                                       scope.ServiceProvider.GetRequiredService<IMovementManager>(),
                                       scope.ServiceProvider.GetRequiredService<ICountryProvider>(),
                                       scope.ServiceProvider.GetRequiredService<IMapProvider>(),
                                       scope.ServiceProvider.GetRequiredService<IAIManager>(),
                                       scope.ServiceProvider.GetRequiredService<ISpeedManager>(),
                                       scope.ServiceProvider.GetRequiredService<IAttackManager>(),
                                       scope.ServiceProvider.GetRequiredService<IStatsManager>());
                }
                else
                {
                    npc = new Npc(_logger,
                                  dbNpc,
                                  moveCoordinates,
                                  scope.ServiceProvider.GetRequiredService<IMovementManager>(),
                                  scope.ServiceProvider.GetRequiredService<ICountryProvider>(),
                                  scope.ServiceProvider.GetRequiredService<IMapProvider>());
                }

                return npc;
            }
            else
            {
                _logger.LogWarning($"Unknown npc type {id.Type} and type id {id.TypeId}");
                return null;
            }
        }
    }
}
