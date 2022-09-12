using Imgeneus.GameDefinitions.Constants;
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
using System.Collections.Generic;

namespace Imgeneus.World.Game.NPCs
{
    public class GuardNpc : Npc, IKiller
    {
        public IAIManager AIManager { get; private set; }
        public ISpeedManager SpeedManager { get; private set; }
        public IAttackManager AttackManager { get; private set; }
        public IStatsManager StatsManager { get; private set; }

        public GuardNpc(ILogger<Npc> logger, BaseNpc npc, List<(float X, float Y, float Z, ushort Angle)> moveCoordinates, IMovementManager movementManager, ICountryProvider countryProvider, IMapProvider mapProvider, IAIManager aiManager, ISpeedManager speedManager, IAttackManager attackManager, IStatsManager statsManager) : base(logger, npc, moveCoordinates, movementManager, countryProvider, mapProvider)
        {
            AIManager = aiManager;
            SpeedManager = speedManager;
            AttackManager = attackManager;
            StatsManager = statsManager;
        }

        public ILevelProvider LevelProvider => throw new System.NotImplementedException();

        public ISkillsManager SkillsManager => throw new System.NotImplementedException();

        public override void Init(uint ownerId)
        {
            base.Init(ownerId);
            AIManager.Init(Id,
                           MobAI.Guard,
                           new MoveArea(_moveCoordinates[0].X, _moveCoordinates[0].Y, _moveCoordinates[0].Z, _moveCoordinates[0].X, _moveCoordinates[0].Y, _moveCoordinates[0].Z),
                           chaseTime: 400,
                           chaseSpeed: 6,
                           chaseRange: 15,
                           isAttack1Enabled: true,
                           attack1Range: 1,
                           attackTime1: 800);
            StatsManager.Init(Id, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue);
            AttackManager.Init(Id);
            AttackManager.WeaponAttackRange = 1;
        }
    }
}
