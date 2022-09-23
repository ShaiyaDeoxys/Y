using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker.Attributes;
using System;
using System.Diagnostics;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MoveCharacterHandler : BaseHandler
    {
        private readonly ILogger<MoveCharacterHandler> _logger;
        private readonly IBuffsManager _buffsManager;
        private readonly IMovementManager _movementManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly ISkillsManager _skillsManager;
        private readonly ISpeedManager _speedManager;

        public MoveCharacterHandler(ILogger<MoveCharacterHandler> logger, IGamePacketFactory packetFactory, IGameSession gameSession, IBuffsManager buffsManager, IMovementManager movementManager, ITeleportationManager teleportationManager, ISkillsManager skillsManager, ISpeedManager speedManager) : base(packetFactory, gameSession)
        {
            _logger = logger;
            _buffsManager = buffsManager;
            _movementManager = movementManager;
            _teleportationManager = teleportationManager;
            _skillsManager = skillsManager;
            _speedManager = speedManager;
        }

        [HandlerAction(PacketType.CHARACTER_MOVE)]
        public void Handle(WorldClient client, MoveCharacterPacket packet)
        {
            //_logger.LogInformation("Character {id} is moving {x} {y} {z}", _gameSession.Character.Id, packet.X, packet.Y, packet.Z);

            if (_teleportationManager.IsTeleporting)
            {
                _logger.LogWarning("Character {id} is moving during teleport.", _gameSession.Character.Id);
                return;
            }

            if (_speedManager.Immobilize)
            {
                _logger.LogWarning("Character {id} is moving during stun. Probably cheating?", _gameSession.Character.Id);
                return;
            }

            var distance = MathExtensions.Distance(_movementManager.PosX, packet.X, _movementManager.PosZ, packet.Z);
            if ((distance > 6 && !_skillsManager.ChargeUsedLastTime.HasValue) || (distance > 6 && DateTime.UtcNow.Subtract(_skillsManager.ChargeUsedLastTime.Value).TotalSeconds > 2))
            {
                _logger.LogWarning("Character {id} is moving too fast. Probably cheating?", _gameSession.Character.Id);
                return;
            }

            _movementManager.PosX = packet.X;
            _movementManager.PosY = packet.Y;
            _movementManager.PosZ = packet.Z;
            _movementManager.Angle = packet.Angle;
            _movementManager.MoveMotion = (MoveMotion)packet.MoveMotion;

            _movementManager.RaisePositionChanged();
            //_logger.LogInformation("Character {id} new position: {x} {y} {z}", _gameSession.Character.Id, _movementManager.PosX, _movementManager.PosY, _movementManager.PosZ);
        }
    }
}
