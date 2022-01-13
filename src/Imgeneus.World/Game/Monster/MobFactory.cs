using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Imgeneus.World.Game.Monster
{
    public class MobFactory : IMobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<Mob, IServiceScope> _mobScopes = new Dictionary<Mob, IServiceScope>(); 

        public MobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Mob CreateMob(ushort mobId, bool shouldRebirth, MoveArea moveArea, Map map)
        {
            var scope = _serviceProvider.CreateScope();

            var mob = new Mob(mobId,
                              shouldRebirth,
                              moveArea,
                              map,
                              scope.ServiceProvider.GetRequiredService<ILogger<Mob>>(),
                              scope.ServiceProvider.GetRequiredService<IDatabasePreloader>(),
                              scope.ServiceProvider.GetRequiredService<IStatsManager>(),
                              scope.ServiceProvider.GetRequiredService<IHealthManager>(),
                              scope.ServiceProvider.GetRequiredService<ILevelProvider>(),
                              scope.ServiceProvider.GetRequiredService<ISkillsManager>());
            mob.OnDead += Mob_OnDead;

            _mobScopes.Add(mob, scope);

            return mob;
        }

        private void Mob_OnDead(IKillable sender, IKiller killer)
        {
            var mob = sender as Mob;
            if (mob.ShouldRebirth)
                return;

            mob.OnDead -= Mob_OnDead;
            _mobScopes.Remove(mob, out var scope);
            scope.Dispose();
            scope = null;
        }
    }
}
