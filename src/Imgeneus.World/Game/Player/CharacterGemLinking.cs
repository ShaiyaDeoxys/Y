using Imgeneus.Database.Constants;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Zone;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        public void AddGem(byte bag, byte slot, byte destinationBag, byte destinationSlot, byte hammerBag, byte hammerSlot)
        {
            
        }

        public void RemoveGem(byte bag, byte slot, bool shouldRemoveSpecificGem, byte gemPosition, byte hammerBag, byte hammerSlot)
        {
            InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
                return;

            bool success = false;
            int spentGold = 0;
            var gemItems = new List<Item>() { null, null, null, null, null, null };
            var savedGems = new List<Gem>();
            var removedGems = new List<Gem>();
            if (shouldRemoveSpecificGem)
            {
                Gem gem = null;
                switch (gemPosition)
                {
                    case 0:
                        gem = item.Gem1;
                        item.Gem1 = null;
                        break;

                    case 1:
                        gem = item.Gem2;
                        item.Gem2 = null;
                        break;

                    case 2:
                        gem = item.Gem3;
                        item.Gem3 = null;
                        break;

                    case 3:
                        gem = item.Gem4;
                        item.Gem4 = null;
                        break;

                    case 4:
                        gem = item.Gem5;
                        item.Gem5 = null;
                        break;

                    case 5:
                        gem = item.Gem6;
                        item.Gem6 = null;
                        break;
                }

                if (gem is null)
                    return;

                InventoryItems.TryGetValue((hammerBag, hammerSlot), out var hammer);
                if (hammer != null)
                    InventoryManager.TryUseItem(hammer.Bag, hammer.Slot);

                success = LinkingManager.RemoveGem(item, gem, hammer);
                spentGold += LinkingManager.GetRemoveGold(gem);

                if (success)
                {
                    savedGems.Add(gem);
                    var gemItem = new Item(_databasePreloader, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                    AddItemToInventory(gemItem);

                    if (gemItem != null)
                        gemItems[gem.Position] = gemItem;
                    //else // Not enough place in inventory.
                    // Map.AddItem(); ?
                }
                removedGems.Add(gem);
            }
            else
            {
                var gems = new List<Gem>();

                if (item.Gem1 != null)
                    gems.Add(item.Gem1);

                if (item.Gem2 != null)
                    gems.Add(item.Gem2);

                if (item.Gem3 != null)
                    gems.Add(item.Gem3);

                if (item.Gem4 != null)
                    gems.Add(item.Gem4);

                if (item.Gem5 != null)
                    gems.Add(item.Gem5);

                if (item.Gem6 != null)
                    gems.Add(item.Gem6);

                foreach (var gem in gems)
                {
                    success = LinkingManager.RemoveGem(item, gem, null);
                    spentGold += LinkingManager.GetRemoveGold(gem);

                    if (success)
                    {
                        savedGems.Add(gem);
                        var gemItem = new Item(_databasePreloader, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                        AddItemToInventory(gemItem);

                        if (gemItem != null)
                            gemItems[gem.Position] = gemItem;
                        //else // Not enough place in inventory.
                        // Map.AddItem(); ?
                    }
                }

                removedGems.AddRange(gems);
                gemPosition = 255; // when remove all gems
            }

            InventoryManager.Gold = (uint)(InventoryManager.Gold - spentGold);

            _packetsHelper.SendRemoveGem(Client, gemItems.Count > 0, item, gemPosition, gemItems, InventoryManager.Gold);

            bool itemDestroyed = false;
            foreach (var gem in removedGems)
            {
                if (gem.ReqVg > 0 && !savedGems.Contains(gem))
                {
                    itemDestroyed = true;
                    break;
                }
            }

            if (item.Bag == 0)
            {
                if (itemDestroyed)
                {
                    /*if (item == Helmet)
                        Helmet = null;
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
                        Costume = null;*/
                }
                else
                {
                    foreach (var gem in removedGems)
                    {
                        StatsManager.ExtraStr -= gem.Str;
                        StatsManager.ExtraDex -= gem.Dex;
                        StatsManager.ExtraRec -= gem.Rec;
                        StatsManager.ExtraInt -= gem.Int;
                        StatsManager.ExtraLuc -= gem.Luc;
                        StatsManager.ExtraWis -= gem.Wis;
                        HealthManager.ExtraHP -= gem.HP;
                        HealthManager.ExtraSP -= gem.SP;
                        HealthManager.ExtraMP -= gem.MP;
                        StatsManager.ExtraDefense -= gem.Defense;
                        StatsManager.ExtraResistance -= gem.Resistance;
                        StatsManager.Absorption -= gem.Absorb;
                        SpeedManager.ExtraMoveSpeed -= gem.MoveSpeed;
                        SpeedManager.ExtraAttackSpeed -= gem.AttackSpeed;

                        //if (gem.Str != 0 || gem.Dex != 0 || gem.Rec != 0 || gem.Wis != 0 || gem.Int != 0 || gem.Luc != 0 || gem.MinAttack != 0 || gem.PlusAttack != 0)
                            //SendAdditionalStats();

                        //if (gem.AttackSpeed != 0 || gem.MoveSpeed != 0)
                            //InvokeAttackOrMoveChanged();
                    }
                }
            }

            // Send gem update to db.
            if (itemDestroyed)
            {
                RemoveItemFromInventory(item);
                SendRemoveItemFromInventory(item, true);
            }
            else
            {
                foreach (var gem in removedGems)
                {
                    switch (gem.Position)
                    {
                        case 0:
                            item.Gem1 = null;
                            break;

                        case 1:
                            item.Gem2 = null;
                            break;

                        case 2:
                            item.Gem3 = null;
                            break;

                        case 3:
                            item.Gem4 = null;
                            break;

                        case 4:
                            item.Gem5 = null;
                            break;

                        case 5:
                            item.Gem6 = null;
                            break;
                    }
                    _taskQueue.Enqueue(ActionType.UPDATE_GEM, Id, item.Bag, item.Slot, gem.Position, 0);
                }
            }
        }

        public void GemRemovePossibility(byte bag, byte slot, bool shouldRemoveSpecificGem, byte gemPosition, byte hammerBag, byte hammerSlot)
        {
            InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
                return;

            double rate = 0;
            int gold = 0;

            if (shouldRemoveSpecificGem)
            {
                Gem gem = null;
                switch (gemPosition)
                {
                    case 0:
                        gem = item.Gem1;
                        break;

                    case 1:
                        gem = item.Gem2;
                        break;

                    case 2:
                        gem = item.Gem3;
                        break;

                    case 3:
                        gem = item.Gem4;
                        break;

                    case 4:
                        gem = item.Gem5;
                        break;

                    case 5:
                        gem = item.Gem6;
                        break;
                }

                if (gem is null)
                    return;

                InventoryItems.TryGetValue((hammerBag, hammerSlot), out var hammer);

                rate = LinkingManager.GetRemoveRate(gem, hammer);
                gold = LinkingManager.GetRemoveGold(gem);
            }
            else
            {
                var gems = new List<Gem>();

                if (item.Gem1 != null)
                    gems.Add(item.Gem1);
                if (item.Gem2 != null)
                    gems.Add(item.Gem2);
                if (item.Gem3 != null)
                    gems.Add(item.Gem3);
                if (item.Gem4 != null)
                    gems.Add(item.Gem4);
                if (item.Gem5 != null)
                    gems.Add(item.Gem5);
                if (item.Gem6 != null)
                    gems.Add(item.Gem6);

                foreach (var gem in gems)
                {
                    rate *= LinkingManager.GetRemoveRate(gem, null) / 100;
                    gold += LinkingManager.GetRemoveGold(gem);
                }

                rate = rate * 100;
            }

            _packetsHelper.SendGemRemovePossibility(Client, rate, gold);
        }
    }
}
