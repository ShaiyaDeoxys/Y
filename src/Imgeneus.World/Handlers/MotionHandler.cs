using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class MotionHandler : BaseHandler
    {
        private readonly IGameWorld _gameWorld;

        public MotionHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameWorld gameWorld) : base(packetFactory, gameSession)
        {
            _gameWorld = gameWorld;
        }

        [HandlerAction(PacketType.CHARACTER_MOTION)]
        public void Handle(WorldClient client, MotionPacket packet)
        {
            var character = _gameWorld.Players[_gameSession.Character.Id];
            character.MovementManager.Motion = packet.Motion;

            _gameSession.StopLogOff();
        }
    }
}
