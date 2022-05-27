using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using Sylver.HandlerInvoker.Attributes;
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

        public MoveCharacterHandler(ILogger<MoveCharacterHandler> logger, IGamePacketFactory packetFactory, IGameSession gameSession, IBuffsManager buffsManager, IMovementManager movementManager, ITeleportationManager teleportationManager) : base(packetFactory, gameSession)
        {
            _logger = logger;
            _buffsManager = buffsManager;
            _movementManager = movementManager;
            _teleportationManager = teleportationManager;
        }

        [HandlerAction(PacketType.CHARACTER_MOVE)]
        public void Handle(WorldClient client, MoveCharacterPacket packet)
        {
            if (_teleportationManager.IsTeleporting)
                return;

            if (_buffsManager.ActiveBuffs.Any(b => b.StateType == StateType.Immobilize || b.StateType == StateType.Sleep || b.StateType == StateType.Stun))
                return;

            var distance = MathExtensions.Distance(_movementManager.PosX, packet.X, _movementManager.PosZ, packet.Z);
            if (distance > 6)
            {
                _logger.LogWarning("Character {id} is moving too fast. Probably cheating?", _gameSession.CharId);
                return;
            }

            _movementManager.PosX = packet.X;
            _movementManager.PosY = packet.Y;
            _movementManager.PosZ = packet.Z;
            _movementManager.Angle = packet.Angle;

            _movementManager.RaisePositionChanged();
        }
    }
}
