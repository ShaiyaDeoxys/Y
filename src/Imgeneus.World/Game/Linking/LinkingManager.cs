using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.Linking
{
    public class LinkingManager : ILinkingManager
    {
        private readonly Random _random = new Random();

        private readonly ILogger<LinkingManager> _logger;
        private readonly IDatabasePreloader _databasePreloader;
        private readonly IInventoryManager _inventoryManager;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ISpeedManager _speedManager;

        public LinkingManager(ILogger<LinkingManager> logger, IDatabasePreloader databasePreloader, IInventoryManager inventoryManager, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager)
        {
            _logger = logger;
            _databasePreloader = databasePreloader;
            _inventoryManager = inventoryManager;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _speedManager = speedManager;

#if DEBUG
            _logger.LogDebug("LinkingManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~LinkingManager()
        {
            _logger.LogDebug("LinkingManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        public Item Item { get; set; }

        public (bool Success, byte Slot, Item Gem, Item Item, Item Hammer) AddGem(byte bag, byte slot, byte destinationBag, byte destinationSlot, byte hammerBag, byte hammerSlot)
        {
            _inventoryManager.InventoryItems.TryGetValue((bag, slot), out var gem);
            if (gem is null || gem.Type != Item.GEM_ITEM_TYPE)
                return (false, 0, null, null, null);

            var linkingGold = GetGold(gem);
            if (_inventoryManager.Gold < linkingGold)
            {
                // TODO: send warning, that not enough money?
                return (false, 0, null, null, null);
            }

            _inventoryManager.InventoryItems.TryGetValue((destinationBag, destinationSlot), out var item);
            if (item is null || item.FreeSlots == 0 || item.ContainsGem(gem.TypeId))
                return (false, 0, null, null, null);

            Item hammer = null;
            if (hammerBag != 0)
                _inventoryManager.InventoryItems.TryGetValue((hammerBag, hammerSlot), out hammer);

            Item saveItem = null;
            if (gem.ReqVg > 0)
            {
                saveItem = _inventoryManager.InventoryItems.Select(itm => itm.Value).FirstOrDefault(itm => itm.Special == SpecialEffect.LuckyCharm);
                if (saveItem != null)
                    _inventoryManager.TryUseItem(saveItem.Bag, saveItem.Slot);
            }

            if (hammer != null)
                _inventoryManager.TryUseItem(hammer.Bag, hammer.Slot);

            _inventoryManager.Gold = (uint)(_inventoryManager.Gold - linkingGold);

            var result = AddGem(item, gem, hammer);

            if (result.Success && item.Bag == 0)
            {
                _statsManager.ExtraStr += gem.Str;
                _statsManager.ExtraDex += gem.Dex;
                _statsManager.ExtraRec += gem.Rec;
                _statsManager.ExtraInt += gem.Int;
                _statsManager.ExtraLuc += gem.Luc;
                _statsManager.ExtraWis += gem.Wis;
                _healthManager.ExtraHP += gem.HP;
                _healthManager.ExtraSP += gem.SP;
                _healthManager.ExtraMP += gem.MP;
                _statsManager.ExtraDefense += gem.Defense;
                _statsManager.ExtraResistance += gem.Resistance;
                _statsManager.Absorption += gem.Absorb;
                _speedManager.ExtraMoveSpeed += gem.MoveSpeed;
                _speedManager.ExtraAttackSpeed += gem.AttackSpeed;

                if (gem.Str != 0 || gem.Dex != 0 || gem.Rec != 0 || gem.Wis != 0 || gem.Int != 0 || gem.Luc != 0 || gem.MinAttack != 0 || gem.MaxAttack != 0)
                    _statsManager.RaiseAdditionalStatsUpdate();
            }

            if (!result.Success && saveItem == null && gem.ReqVg > 0)
            {
                _inventoryManager.RemoveItem(item);

                if (item.Bag == 0)
                {
                    if (item == _inventoryManager.Helmet)
                        _inventoryManager.Helmet = null;
                    else if (item == _inventoryManager.Armor)
                        _inventoryManager.Armor = null;
                    else if (item == _inventoryManager.Pants)
                        _inventoryManager.Pants = null;
                    else if (item == _inventoryManager.Gauntlet)
                        _inventoryManager.Gauntlet = null;
                    else if (item == _inventoryManager.Boots)
                        _inventoryManager.Boots = null;
                    else if (item == _inventoryManager.Weapon)
                        _inventoryManager.Weapon = null;
                    else if (item == _inventoryManager.Shield)
                        _inventoryManager.Shield = null;
                    else if (item == _inventoryManager.Cape)
                        _inventoryManager.Cape = null;
                    else if (item == _inventoryManager.Amulet)
                        _inventoryManager.Amulet = null;
                    else if (item == _inventoryManager.Ring1)
                        _inventoryManager.Ring1 = null;
                    else if (item == _inventoryManager.Ring2)
                        _inventoryManager.Ring2 = null;
                    else if (item == _inventoryManager.Bracelet1)
                        _inventoryManager.Bracelet1 = null;
                    else if (item == _inventoryManager.Bracelet2)
                        _inventoryManager.Bracelet2 = null;
                    else if (item == _inventoryManager.Mount)
                        _inventoryManager.Mount = null;
                    else if (item == _inventoryManager.Pet)
                        _inventoryManager.Pet = null;
                    else if (item == _inventoryManager.Costume)
                        _inventoryManager.Costume = null;
                }
            }

            return (result.Success, result.Slot, gem, item, hammer);
        }

        private (bool Success, byte Slot) AddGem(Item item, Item gem, Item hammer)
        {
            double rate = GetRate(gem, hammer);
            var rand = _random.Next(1, 101);
            var success = rate >= rand;
            byte slot = 0;
            if (success)
            {
                if (item.Gem1 is null)
                {
                    slot = 0;
                    item.Gem1 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 1;
                    item.Gem2 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
                else if (item.Gem3 is null)
                {
                    slot = 2;
                    item.Gem3 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
                else if (item.Gem4 is null)
                {
                    slot = 3;
                    item.Gem4 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 4;
                    item.Gem5 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 5;
                    item.Gem6 = new Gem(_databasePreloader, gem.TypeId, slot);
                }
            }
            gem.Count--;
            return (success, slot);
        }

        public (bool Success, byte Slot, List<Item> SavedGems, Item Item) RemoveGem(byte bag, byte slot, bool shouldRemoveSpecificGem, byte gemPosition, byte hammerBag, byte hammerSlot)
        {
            bool success = false;
            int spentGold = 0;
            var gemItems = new List<Item>() { null, null, null, null, null, null };
            var savedGems = new List<Gem>();
            var removedGems = new List<Gem>();

            _inventoryManager.InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
                return (success, 0, gemItems, null);

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
                    return (success, 0, gemItems, null);

                _inventoryManager.InventoryItems.TryGetValue((hammerBag, hammerSlot), out var hammer);
                if (hammer != null)
                    _inventoryManager.TryUseItem(hammer.Bag, hammer.Slot);

                success = RemoveGem(item, gem, hammer);
                spentGold += GetRemoveGold(gem);

                if (success)
                {
                    savedGems.Add(gem);
                    var gemItem = new Item(_databasePreloader, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                    _inventoryManager.AddItem(gemItem);

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
                    success = RemoveGem(item, gem, null);
                    spentGold += GetRemoveGold(gem);

                    if (success)
                    {
                        savedGems.Add(gem);
                        var gemItem = new Item(_databasePreloader, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                        _inventoryManager.AddItem(gemItem);

                        if (gemItem != null)
                            gemItems[gem.Position] = gemItem;
                        //else // Not enough place in inventory.
                        // Map.AddItem(); ?
                    }
                }

                removedGems.AddRange(gems);
                gemPosition = 255; // when remove all gems
            }

            _inventoryManager.Gold = (uint)(_inventoryManager.Gold - spentGold);

            var itemDestroyed = false;
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
                    if (item == _inventoryManager.Helmet)
                        _inventoryManager.Helmet = null;
                    else if (item == _inventoryManager.Armor)
                        _inventoryManager.Armor = null;
                    else if (item == _inventoryManager.Pants)
                        _inventoryManager.Pants = null;
                    else if (item == _inventoryManager.Gauntlet)
                        _inventoryManager.Gauntlet = null;
                    else if (item == _inventoryManager.Boots)
                        _inventoryManager.Boots = null;
                    else if (item == _inventoryManager.Weapon)
                        _inventoryManager.Weapon = null;
                    else if (item == _inventoryManager.Shield)
                        _inventoryManager.Shield = null;
                    else if (item == _inventoryManager.Cape)
                        _inventoryManager.Cape = null;
                    else if (item == _inventoryManager.Amulet)
                        _inventoryManager.Amulet = null;
                    else if (item == _inventoryManager.Ring1)
                        _inventoryManager.Ring1 = null;
                    else if (item == _inventoryManager.Ring2)
                        _inventoryManager.Ring2 = null;
                    else if (item == _inventoryManager.Bracelet1)
                        _inventoryManager.Bracelet1 = null;
                    else if (item == _inventoryManager.Bracelet2)
                        _inventoryManager.Bracelet2 = null;
                    else if (item == _inventoryManager.Mount)
                        _inventoryManager.Mount = null;
                    else if (item == _inventoryManager.Pet)
                        _inventoryManager.Pet = null;
                    else if (item == _inventoryManager.Costume)
                        _inventoryManager.Costume = null;
                }
                else
                {
                    foreach (var gem in removedGems)
                    {
                        _statsManager.ExtraStr -= gem.Str;
                        _statsManager.ExtraDex -= gem.Dex;
                        _statsManager.ExtraRec -= gem.Rec;
                        _statsManager.ExtraInt -= gem.Int;
                        _statsManager.ExtraLuc -= gem.Luc;
                        _statsManager.ExtraWis -= gem.Wis;
                        _healthManager.ExtraHP -= gem.HP;
                        _healthManager.ExtraSP -= gem.SP;
                        _healthManager.ExtraMP -= gem.MP;
                        _statsManager.ExtraDefense -= gem.Defense;
                        _statsManager.ExtraResistance -= gem.Resistance;
                        _statsManager.Absorption -= gem.Absorb;
                        _speedManager.ExtraMoveSpeed -= gem.MoveSpeed;
                        _speedManager.ExtraAttackSpeed -= gem.AttackSpeed;

                        if (gem.Str != 0 || gem.Dex != 0 || gem.Rec != 0 || gem.Wis != 0 || gem.Int != 0 || gem.Luc != 0 || gem.MinAttack != 0 || gem.PlusAttack != 0)
                            _statsManager.RaiseAdditionalStatsUpdate();
                    }
                }
            }

            if (itemDestroyed)
            {
                _inventoryManager.RemoveItem(item);
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
                }
            }

            return (!itemDestroyed, gemPosition, gemItems, item);
        }

        private bool RemoveGem(Item item, Gem gem, Item hammer, byte extraRate = 0)
        {
            var rate = GetRemoveRate(gem, hammer, extraRate);
            var rand = _random.Next(1, 101);
            var success = rate >= rand;

            return success;
        }

        public double GetRate(Item gem, Item hammer)
        {
            double rate = GetRateByReqIg(gem.ReqIg);
            rate += CalculateExtraRate();

            if (hammer != null)
            {
                if (hammer.Special == SpecialEffect.LinkingHammer)
                {
                    rate = rate * (hammer.ReqVg / 100);
                    if (rate > 50)
                        rate = 50;
                }

                if (hammer.Special == SpecialEffect.PerfectLinkingHammer)
                    rate = 100;
            }

            return rate;
        }


        /// <summary>
        /// Extra rate is made of guild house blacksmith rate + bless rate.
        /// </summary>
        /// <returns></returns>
        private byte CalculateExtraRate()
        {
            byte extraRate = 0;
            //if (HasGuild && Map is GuildHouseMap)
            //{
            //    var rates = _guildManager.GetBlacksmithRates((int)GuildId);
            //    extraRate += rates.LinkRate;
            //}

            // TODO: add bless rate.

            return extraRate;
        }

        public int GetGold(Item gem)
        {
            int gold = GetGoldByReqIg(gem.ReqIg);
            return gold;
        }

        public double GetRemoveRate(Gem gem, Item hammer, byte extraRate)
        {
            double rate = GetRateByReqIg(gem.ReqIg);
            rate += extraRate;

            if (hammer != null)
            {
                if (hammer.Special == SpecialEffect.ExtractionHammer)
                {
                    if (hammer.ReqVg == 40) // Small extracting hammer.
                        rate = 40;

                    if (hammer.ReqVg >= 80) // Big extracting hammer.
                        rate = 80;
                }

                if (hammer.Special == SpecialEffect.PerfectExtractionHammer)
                    rate = 100; // GM extracting hammer. Usually it's item with Type = 44 and TypeId = 237 or create your own.
            }

            return rate;
        }

        public int GetRemoveGold(Gem gem)
        {
            int gold = GetGoldByReqIg(gem.ReqIg);
            return gold;
        }

        private double GetRateByReqIg(byte ReqIg)
        {
            double rate;
            switch (ReqIg)
            {
                case 30:
                    rate = 50;
                    break;

                case 31:
                    rate = 46;
                    break;

                case 32:
                    rate = 40;
                    break;

                case 33:
                    rate = 32;
                    break;

                case 34:
                    rate = 24;
                    break;

                case 35:
                    rate = 16;
                    break;

                case 36:
                    rate = 8;
                    break;

                case 37:
                    rate = 2;
                    break;

                case 38:
                    rate = 1;
                    break;

                case 39:
                    rate = 1;
                    break;

                case 40:
                    rate = 1;
                    break;

                case 99:
                    rate = 1;
                    break;

                case 255: // ONLY FOR TESTS!
                    rate = 100;
                    break;

                default:
                    rate = 0;
                    break;
            }

            return rate;
        }

        private int GetGoldByReqIg(byte reqIg)
        {
            int gold;
            switch (reqIg)
            {
                case 30:
                    gold = 1000;
                    break;

                case 31:
                    gold = 4095;
                    break;

                case 32:
                    gold = 11250;
                    break;

                case 33:
                    gold = 22965;
                    break;

                case 34:
                    gold = 41280;
                    break;

                case 35:
                    gold = 137900;
                    break;

                case 36:
                    gold = 365000;
                    break;

                case 37:
                    gold = 480000;
                    break;

                case 38:
                    gold = 627000;
                    break;

                case 39:
                    gold = 814000;
                    break;

                case 40:
                    gold = 1040000;
                    break;

                case 99:
                    gold = 7500000;
                    break;

                default:
                    gold = 0;
                    break;
            }

            return gold;
        }

        public void Compose(Item recRune)
        {
            switch (recRune.Special)
            {
                case SpecialEffect.RecreationRune:
                    RandomCompose();
                    break;

                case SpecialEffect.RecreationRune_STR:
                    ComposeStr();
                    break;

                case SpecialEffect.RecreationRune_DEX:
                    ComposeDex();
                    break;

                case SpecialEffect.RecreationRune_REC:
                    ComposeRec();
                    break;

                case SpecialEffect.RecreationRune_INT:
                    ComposeInt();
                    break;

                case SpecialEffect.RecreationRune_WIS:
                    ComposeWis();
                    break;

                case SpecialEffect.RecreationRune_LUC:
                    ComposeLuc();
                    break;

                default:
                    break;
            }
        }

        private void ComposeStr()
        {
            if (Item.ComposedStr == 0)
                return;

            Item.ComposedStr = _random.Next(1, Item.ReqWis + 1);
        }

        private void ComposeDex()
        {
            if (Item.ComposedDex == 0)
                return;

            Item.ComposedDex = _random.Next(1, Item.ReqWis + 1);
        }

        private void ComposeRec()
        {
            if (Item.ComposedRec == 0)
                return;

            Item.ComposedRec = _random.Next(1, Item.ReqWis + 1);
        }

        private void ComposeInt()
        {
            if (Item.ComposedInt == 0)
                return;

            Item.ComposedInt = _random.Next(1, Item.ReqWis + 1);
        }

        private void ComposeWis()
        {
            if (Item.ComposedWis == 0)
                return;

            Item.ComposedWis = _random.Next(1, Item.ReqWis + 1);
        }

        private void ComposeLuc()
        {
            if (Item.ComposedLuc == 0)
                return;

            Item.ComposedLuc = _random.Next(1, Item.ReqWis + 1);
        }

        private void RandomCompose()
        {
            Item.ComposedStr = 0;
            Item.ComposedDex = 0;
            Item.ComposedRec = 0;
            Item.ComposedInt = 0;
            Item.ComposedWis = 0;
            Item.ComposedLuc = 0;
            Item.ComposedHP = 0;
            Item.ComposedMP = 0;
            Item.ComposedSP = 0;

            var maxIndex = Item.IsWeapon ? 6 : 9; // Weapons can not have hp, mp or sp recreated.
            var indexes = new List<int>();
            do
            {
                var index = _random.Next(0, maxIndex);
                if (!indexes.Contains(index))
                    indexes.Add(index);
            }
            while (indexes.Count != 3);

            foreach (var i in indexes)
            {
                switch (i)
                {
                    case 0:
                        Item.ComposedStr = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 1:
                        Item.ComposedDex = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 2:
                        Item.ComposedRec = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 3:
                        Item.ComposedInt = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 4:
                        Item.ComposedWis = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 5:
                        Item.ComposedLuc = _random.Next(1, Item.ReqWis + 1);
                        break;

                    case 6:
                        Item.ComposedHP = _random.Next(1, Item.ReqWis + 1) * 100;
                        break;

                    case 7:
                        Item.ComposedMP = _random.Next(1, Item.ReqWis + 1) * 100;
                        break;

                    case 8:
                        Item.ComposedSP = _random.Next(1, Item.ReqWis + 1) * 100;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
