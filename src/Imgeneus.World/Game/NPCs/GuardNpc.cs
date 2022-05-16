using Imgeneus.Database.Constants;
using Imgeneus.World.Game.AI;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Levelling;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.World.Game.NPCs
{
    public class GuardNpc : Npc, IKiller
    {
        public IAIManager AIManager { get; private set; }
        public ISpeedManager SpeedManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public IStatsManager StatsManager { get; private set; }

        public GuardNpc(Map map, ILogger<Npc> logger, BaseNpc npc, IMovementManager movementManager, ICountryProvider countryProvider, IAIManager aiManager, ISpeedManager speedManager, IAttackManager attackManager, IStatsManager statsManager) : base(map, logger, npc, movementManager, countryProvider)
        {
            AIManager = aiManager;
            SpeedManager = speedManager;
            AttackManager = attackManager;
            StatsManager = statsManager;
        }

        public ILevelProvider LevelProvider => throw new System.NotImplementedException();

        public ISkillsManager SkillsManager => throw new System.NotImplementedException();
    }
}
