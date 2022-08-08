using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Skills;
using Imgeneus.GameDefinitions;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Game.Teleport;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class RebirthHandler : BaseHandler
    {
        private readonly IMapProvider _mapProvider;
        private readonly IGameWorld _gameWorld;
        private readonly IHealthManager _healthManager;
        private readonly ITeleportationManager _teleportationManager;
        private readonly IMovementManager _movementManager;
        private readonly IBuffsManager _buffsManager;
        private readonly IInventoryManager _inventoryManager;
        private readonly IGameDefinitionsPreloder _definitionsPreloder;

        public RebirthHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider, IGameWorld gameWorld, IHealthManager healthManager, ITeleportationManager teleportationManager, IMovementManager movementManager, IBuffsManager buffsManager, IInventoryManager inventoryManager, IGameDefinitionsPreloder definitionsPreloder) : base(packetFactory, gameSession)
        {
            _mapProvider = mapProvider;
            _gameWorld = gameWorld;
            _healthManager = healthManager;
            _teleportationManager = teleportationManager;
            _movementManager = movementManager;
            _buffsManager = buffsManager;
            _inventoryManager = inventoryManager;
            _definitionsPreloder = definitionsPreloder;
        }

        [HandlerAction(PacketType.REBIRTH_TO_NEAREST_TOWN)]
        public void Handle(WorldClient client, RebirthPacket packet)
        {
            var rebirthType = (RebirthType)packet.RebirthType;

            // TODO: implement other rebith types.

            (ushort MapId, float X, float Y, float Z) rebirthCoordinate = (0, 0, 0, 0);

            // Usual rebirth.
            if (rebirthType == RebirthType.KillSoul)
            {
                rebirthCoordinate = _mapProvider.Map.GetRebirthMap(_gameWorld.Players[_gameSession.Character.Id]);
            }

            // Rebirth with rune. Will rebirth at the same place.
            if (rebirthType == RebirthType.KillSoulByItem)
            {
                var rune = _inventoryManager.InventoryItems.Values.FirstOrDefault(x => x.Special == SpecialEffect.ResurrectionRune);
                if (rune != null)
                {
                    _inventoryManager.TryUseItem(rune.Bag, rune.Slot, skipApplyingItemEffect: true);

                    rebirthCoordinate.MapId = _mapProvider.Map.Id;
                    rebirthCoordinate.X = _movementManager.PosX;
                    rebirthCoordinate.Y = _movementManager.PosY;
                    rebirthCoordinate.Z = _movementManager.PosZ;

                    // Add untouchable buff for 6 secs.
                    _definitionsPreloder.Skills.TryGetValue((199, 2), out var dbSkill);
                    _buffsManager.AddBuff(new Skill(dbSkill, 0, 0), null);
                }
                else
                {
                    rebirthCoordinate = _mapProvider.Map.GetRebirthMap(_gameWorld.Players[_gameSession.Character.Id]);
                }
            }

            if (_mapProvider.Map.Id != rebirthCoordinate.MapId)
                _teleportationManager.Teleport(rebirthCoordinate.MapId, rebirthCoordinate.X, rebirthCoordinate.Y, rebirthCoordinate.Z);
            else
            {
                _movementManager.PosX = rebirthCoordinate.X;
                _movementManager.PosY = rebirthCoordinate.Y;
                _movementManager.PosZ = rebirthCoordinate.Z;
            }

            _healthManager.Rebirth();
        }
    }
}
