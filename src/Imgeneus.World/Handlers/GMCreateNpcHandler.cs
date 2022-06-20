using Imgeneus.GameDefinitions;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Collections.Generic;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class GMCreateNpcHandler : BaseHandler
    {
        private readonly IGameDefinitionsPreloder _definitionsPreloder;
        private readonly IMovementManager _movementManager;
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly INpcFactory _npcFactory;

        public GMCreateNpcHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IGameDefinitionsPreloder definitionsPreloder, IMovementManager movementManager, IMapProvider mapProvider, IGameWorld gameWorld, INpcFactory npcFactory) : base(packetFactory, gameSession)
        {
            _definitionsPreloder = definitionsPreloder;
            _movementManager = movementManager;
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _npcFactory = npcFactory;
        }

        [HandlerAction(PacketType.GM_CREATE_NPC)]
        public void HandleOriginal(WorldClient client, GMCreateNpcPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_definitionsPreloder.NPCs.ContainsKey((packet.Type, packet.TypeId)))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_CREATE_NPC);
                return;
            }

            Handle(client, packet);
        }

        [HandlerAction(PacketType.GM_SHAIYA_US_CREATE_NPC)]
        public void HandleUs(WorldClient client, GMCreateNpcPacket packet)
        {
            if (!_gameSession.IsAdmin)
                return;

            if (!_definitionsPreloder.NPCs.ContainsKey((packet.Type, packet.TypeId)))
            {
                _packetFactory.SendGmCommandError(client, PacketType.GM_SHAIYA_US_CREATE_NPC);
                return;
            }

            Handle(client, packet);
        }

        private void Handle(WorldClient client, GMCreateNpcPacket packet)
        {
            var moveCoordinates = new List<(float, float, float, ushort)>()
                        {
                            (_movementManager.PosX, _movementManager.PosY - 1, _movementManager.PosZ, _movementManager.Angle)
                        };

            var npc = _npcFactory.CreateNpc((packet.Type, packet.TypeId), moveCoordinates, _mapProvider.Map);
            npc.Init(_mapProvider.Map.GenerateId());
            npc.Map = _mapProvider.Map;


            _mapProvider.Map.AddNPC(_gameWorld.Players[_gameSession.Character.Id].CellId, npc);
            _packetFactory.SendGmCommandSuccess(client);
        }
    }
}
