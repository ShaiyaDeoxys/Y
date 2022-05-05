using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Elements;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Untouchable;
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

        private readonly Dictionary<int, IServiceScope> _mobScopes = new Dictionary<int, IServiceScope>();

        private readonly Dictionary<int, Mob> _mobs = new Dictionary<int, Mob>();

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
                              scope.ServiceProvider.GetRequiredService<IItemEnchantConfiguration>(),
                              scope.ServiceProvider.GetRequiredService<IItemCreateConfiguration>(),
                              scope.ServiceProvider.GetRequiredService<ICountryProvider>(),
                              scope.ServiceProvider.GetRequiredService<IStatsManager>(),
                              scope.ServiceProvider.GetRequiredService<IHealthManager>(),
                              scope.ServiceProvider.GetRequiredService<ILevelProvider>(),
                              scope.ServiceProvider.GetRequiredService<ISpeedManager>(),
                              scope.ServiceProvider.GetRequiredService<IAttackManager>(),
                              scope.ServiceProvider.GetRequiredService<ISkillsManager>(),
                              scope.ServiceProvider.GetRequiredService<IBuffsManager>(),
                              scope.ServiceProvider.GetRequiredService<IElementProvider>(),
                              scope.ServiceProvider.GetRequiredService<IMovementManager>(),
                              scope.ServiceProvider.GetRequiredService<IUntouchableManager>(),
                              scope.ServiceProvider.GetRequiredService<IMapProvider>());

            if (!mob.ShouldRebirth)
            {
                mob.HealthManager.OnDead += Mob_OnDead;
                _mobScopes.Add(mob.Id, scope);
                _mobs.Add(mob.Id, mob);
            }

            return mob;
        }

        private void Mob_OnDead(int senderId, IKiller killer)
        {
            _mobs.TryGetValue(senderId, out var mob);

            mob.HealthManager.OnDead -= Mob_OnDead;
            _mobs.Remove(senderId);

            _mobScopes.Remove(senderId, out var scope);
            scope.Dispose();
            scope = null;
        }
    }
}
