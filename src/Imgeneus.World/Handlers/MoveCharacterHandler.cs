using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MoveCharacterHandler : BaseHandler
    {
        private readonly IBuffsManager _buffsManager;
        private readonly IMovementManager _movementManager;

        public MoveCharacterHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IBuffsManager buffsManager, IMovementManager movementManager) : base(packetFactory, gameSession)
        {
            _buffsManager = buffsManager;
            _movementManager = movementManager;
        }

        [HandlerAction(PacketType.CHARACTER_MOVE)]
        public void Handle(WorldClient client, MoveCharacterPacket packet)
        {
            //if (IsTeleporting)
            //    return;

            if (_buffsManager.ActiveBuffs.Any(b => b.StateType == StateType.Immobilize || b.StateType == StateType.Sleep || b.StateType == StateType.Stun))
                return;

            _movementManager.PosX = packet.X;
            _movementManager.PosY = packet.Y;
            _movementManager.PosZ = packet.Z;
            _movementManager.Angle = packet.Angle;

            _movementManager.RaisePositionChanged();

            //if (IsDuelApproved && MathExtensions.Distance(PosX, DuelX, PosZ, DuelZ) >= 45)
            //{
            //    FinishDuel(DuelCancelReason.TooFarAway);
            //}
        }
    }
}
