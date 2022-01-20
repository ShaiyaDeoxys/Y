using Imgeneus.Database.Constants;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Linking;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Linq;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class AddGemHandler : BaseHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly ILinkingManager _linkingManager;

        public AddGemHandler(IGamePacketFactory packetFactory, IGameSession gameSession, IInventoryManager inventoryManager, ILinkingManager linkingManager) : base(packetFactory, gameSession)
        {
            _inventoryManager = inventoryManager;
            _linkingManager = linkingManager;
        }

        [HandlerAction(PacketType.GEM_ADD)]
        public void Handle(WorldClient client, GemAddPacket packet)
        {
            _inventoryManager.InventoryItems.TryGetValue((packet.Bag, packet.Slot), out var gem);
            if (gem is null || gem.Type != Item.GEM_ITEM_TYPE)
                return;

            var linkingGold = _linkingManager.GetGold(gem);
            if (_inventoryManager.Gold < linkingGold)
            {
                // TODO: send warning, that not enough money?
                return;
            }

            _inventoryManager.InventoryItems.TryGetValue((packet.DestinationBag, packet.DestinationSlot), out var item);
            if (item is null || item.FreeSlots == 0 || item.ContainsGem(gem.TypeId))
                return;

            Item hammer = null;
            if (packet.HammerBag != 0)
                _inventoryManager.InventoryItems.TryGetValue((packet.HammerBag, packet.HammerSlot), out hammer);

            Item saveItem = null;
            if (gem.ReqVg > 0)
            {
                saveItem = _inventoryManager.InventoryItems.Select(itm => itm.Value).FirstOrDefault(itm => itm.Special == SpecialEffect.LuckyCharm);
                //if (saveItem != null)
                    //_inventoryManager.TryUseItem(saveItem.Bag, saveItem.Slot);
            }

            var result = _linkingManager.AddGem(item, gem, hammer);
            _inventoryManager.Gold = (uint)(_inventoryManager.Gold - linkingGold);
            /*if (gem.Count > 0)
            {
                _taskQueue.Enqueue(ActionType.UPDATE_ITEM_COUNT_IN_INVENTORY,
                                   Id, gem.Bag, gem.Slot, gem.Count);
            }
            else
            {
                InventoryItems.TryRemove((gem.Bag, gem.Slot), out var removedGem);
                _taskQueue.Enqueue(ActionType.REMOVE_ITEM_FROM_INVENTORY,
                                   Id, gem.Bag, gem.Slot);
            }*/

            //if (result.Success)
            //    _taskQueue.Enqueue(ActionType.UPDATE_GEM, Id, item.Bag, item.Slot, result.Slot, (int)gem.TypeId);

            //if (hammer != null)
            //    TryUseItem(hammer.Bag, hammer.Slot);

            //_packetsHelper.SendAddGem(Client, result.Success, gem, item, result.Slot, InventoryManager.Gold, saveItem, hammer);

            /*if (result.Success && item.Bag == 0)
            {
                StatsManager.ExtraStr += gem.Str;
                StatsManager.ExtraDex += gem.Dex;
                StatsManager.ExtraRec += gem.Rec;
                StatsManager.ExtraInt += gem.Int;
                StatsManager.ExtraLuc += gem.Luc;
                StatsManager.ExtraWis += gem.Wis;
                HealthManager.ExtraHP += gem.HP;
                HealthManager.ExtraSP += gem.SP;
                HealthManager.ExtraMP += gem.MP;
                StatsManager.ExtraDefense += gem.Defense;
                StatsManager.ExtraResistance += gem.Resistance;
                StatsManager.Absorption += gem.Absorb;
                SpeedManager.ExtraMoveSpeed += gem.MoveSpeed;
                SpeedManager.ExtraAttackSpeed += gem.AttackSpeed;

                //if (gem.Str != 0 || gem.Dex != 0 || gem.Rec != 0 || gem.Wis != 0 || gem.Int != 0 || gem.Luc != 0 || gem.MinAttack != 0 || gem.MaxAttack != 0)
                //SendAdditionalStats();

                //if (gem.AttackSpeed != 0 || gem.MoveSpeed != 0)
                //InvokeAttackOrMoveChanged();
            }

            if (!result.Success && saveItem == null && gem.ReqVg > 0)
            {
                RemoveItemFromInventory(item);
                SendRemoveItemFromInventory(item, true);

                if (item.Bag == 0)
                {
                    if (item == InventoryManager.Helmet)
                        InventoryManager.Helmet = null;
                    else if (item == Armor)
                        Armor = null;
                    else if (item == Pants)
                        Pants = null;
                    else if (item == Gauntlet)
                        Gauntlet = null;
                    else if (item == Boots)
                        Boots = null;
                    else if (item == Weapon)
                        Weapon = null;
                    else if (item == Shield)
                        Shield = null;
                    else if (item == Cape)
                        Cape = null;
                    else if (item == Amulet)
                        Amulet = null;
                    else if (item == Ring1)
                        Ring1 = null;
                    else if (item == Ring2)
                        Ring2 = null;
                    else if (item == Bracelet1)
                        Bracelet1 = null;
                    else if (item == Bracelet2)
                        Bracelet2 = null;
                    else if (item == Mount)
                        Mount = null;
                    else if (item == Pet)
                        Pet = null;
                    else if (item == Costume)
                        Costume = null;
                }
            }*/
        }
    }
}
