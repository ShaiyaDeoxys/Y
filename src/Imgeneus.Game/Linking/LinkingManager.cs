using Imgeneus.Database.Constants;
using Imgeneus.Database.Preload;
using Imgeneus.Game.Blessing;
using Imgeneus.GameDefinitions;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Guild;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Speed;
using Imgeneus.World.Game.Stats;
using Imgeneus.World.Game.Zone;
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
        private readonly IGameDefinitionsPreloder _definitionsPreloader;
        private readonly IInventoryManager _inventoryManager;
        private readonly IStatsManager _statsManager;
        private readonly IHealthManager _healthManager;
        private readonly ISpeedManager _speedManager;
        private readonly IGuildManager _guildManager;
        private readonly IMapProvider _mapProvider;
        private readonly IItemEnchantConfiguration _itemEnchantConfig;
        private readonly IItemCreateConfiguration _itemCreateConfig;
        private readonly ICountryProvider _countryProvider;
        private readonly IBlessManager _blessManager;

        public LinkingManager(ILogger<LinkingManager> logger, IGameDefinitionsPreloder definitionsPreloader, IInventoryManager inventoryManager, IStatsManager statsManager, IHealthManager healthManager, ISpeedManager speedManager, IGuildManager guildManager, IMapProvider mapProvider, IItemEnchantConfiguration itemEnchantConfig, IItemCreateConfiguration itemCreateConfig, ICountryProvider countryProvider, IBlessManager blessManager)
        {
            _logger = logger;
            _definitionsPreloader = definitionsPreloader;
            _inventoryManager = inventoryManager;
            _statsManager = statsManager;
            _healthManager = healthManager;
            _speedManager = speedManager;
            _guildManager = guildManager;
            _mapProvider = mapProvider;
            _itemEnchantConfig = itemEnchantConfig;
            _itemCreateConfig = itemCreateConfig;
            _countryProvider = countryProvider;
            _blessManager = blessManager;

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

        #region Gem linking

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
            if (gem.CanBreakItem)
            {
                saveItem = _inventoryManager.InventoryItems.Select(itm => itm.Value).FirstOrDefault(itm => itm.Special == SpecialEffect.LuckyCharm);
                if (saveItem != null)
                    _inventoryManager.TryUseItem(saveItem.Bag, saveItem.Slot, skipApplyingItemEffect: true);
            }

            if (hammer != null)
                _inventoryManager.TryUseItem(hammer.Bag, hammer.Slot, skipApplyingItemEffect: true);

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

            if (!result.Success && saveItem == null && gem.CanBreakItem)
            {
                _inventoryManager.RemoveItem(item, "broken_by_gem");

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
                    item.Gem1 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 1;
                    item.Gem2 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
                else if (item.Gem3 is null)
                {
                    slot = 2;
                    item.Gem3 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
                else if (item.Gem4 is null)
                {
                    slot = 3;
                    item.Gem4 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 4;
                    item.Gem5 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
                else if (item.Gem2 is null)
                {
                    slot = 5;
                    item.Gem6 = new Gem(_definitionsPreloader, gem.TypeId, slot);
                }
            }
            gem.Count--;
            if (gem.Count == 0)
                _inventoryManager.RemoveItem(gem, "used_in_linking");

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
                    _inventoryManager.TryUseItem(hammer.Bag, hammer.Slot, skipApplyingItemEffect: true);

                success = RemoveGem(item, gem, hammer);
                spentGold += GetRemoveGold(gem);

                if (success)
                {
                    savedGems.Add(gem);
                    var gemItem = new Item(_definitionsPreloader, _itemEnchantConfig, _itemCreateConfig, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                    _inventoryManager.AddItem(gemItem, "remove_gem");

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
                        var gemItem = new Item(_definitionsPreloader, _itemEnchantConfig, _itemCreateConfig, Item.GEM_ITEM_TYPE, (byte)gem.TypeId);
                        _inventoryManager.AddItem(gemItem, "remove_gem");

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
                if (gem.LinkingRate > 0 && !savedGems.Contains(gem))
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
                _inventoryManager.RemoveItem(item, "remove_gem_failed");
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

        private bool RemoveGem(Item item, Gem gem, Item hammer)
        {
            var rate = GetRemoveRate(gem, hammer);
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
                    rate = rate * (hammer.LinkingRate / 100);
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
            if (_guildManager.HasGuild && _mapProvider.Map is GuildHouseMap)
            {
                var rates = _guildManager.GetBlacksmithRates();
                extraRate += rates.LinkRate;
            }

            if (_countryProvider.Country == CountryType.Light && _blessManager.LightAmount >= IBlessManager.LINK_EXTRACT_LAPIS)
                extraRate += 2;

            if (_countryProvider.Country == CountryType.Dark && _blessManager.DarkAmount >= IBlessManager.LINK_EXTRACT_LAPIS)
                extraRate += 2;

            return extraRate;
        }

        public int GetGold(Item gem)
        {
            int gold = GetGoldByReqIg(gem.ReqIg);
            return gold;
        }

        public double GetRemoveRate(Gem gem, Item hammer)
        {
            double rate = GetRateByReqIg(gem.ReqIg);
            rate += CalculateExtraRate();

            if (hammer != null)
            {
                if (hammer.Special == SpecialEffect.ExtractionHammer)
                {
                    if (hammer.LinkingRate == 40) // Small extracting hammer.
                        rate = 40;

                    if (hammer.LinkingRate >= 80) // Big extracting hammer.
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

        #endregion

        #region Rec rune composition

        public (bool Success, Item Item) Compose(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot)
        {
            _inventoryManager.InventoryItems.TryGetValue((runeBag, runeSlot), out var rune);
            _inventoryManager.InventoryItems.TryGetValue((itemBag, itemSlot), out var item);

            if (rune is null || item is null ||
                   (rune.Special != SpecialEffect.RecreationRune &&
                    rune.Special != SpecialEffect.RecreationRune_STR &&
                    rune.Special != SpecialEffect.RecreationRune_DEX &&
                    rune.Special != SpecialEffect.RecreationRune_REC &&
                    rune.Special != SpecialEffect.RecreationRune_INT &&
                    rune.Special != SpecialEffect.RecreationRune_WIS &&
                    rune.Special != SpecialEffect.RecreationRune_LUC) ||
                !item.IsComposable)
            {
                return (false, item);
            }

            if (item.Bag == 0)
            {
                _statsManager.ExtraStr -= item.ComposedStr;
                _statsManager.ExtraDex -= item.ComposedDex;
                _statsManager.ExtraRec -= item.ComposedRec;
                _statsManager.ExtraInt -= item.ComposedInt;
                _statsManager.ExtraWis -= item.ComposedWis;
                _statsManager.ExtraLuc -= item.ComposedLuc;
                _healthManager.ExtraHP -= item.ComposedHP;
                _healthManager.ExtraMP -= item.ComposedMP;
                _healthManager.ExtraSP -= item.ComposedSP;
            }

            Compose(item, rune);

            if (item.Bag == 0)
            {
                _statsManager.ExtraStr += item.ComposedStr;
                _statsManager.ExtraDex += item.ComposedDex;
                _statsManager.ExtraRec += item.ComposedRec;
                _statsManager.ExtraInt += item.ComposedInt;
                _statsManager.ExtraWis += item.ComposedWis;
                _statsManager.ExtraLuc += item.ComposedLuc;
                _healthManager.ExtraHP += item.ComposedHP;
                _healthManager.ExtraMP += item.ComposedMP;
                _healthManager.ExtraSP += item.ComposedSP;

                _statsManager.RaiseAdditionalStatsUpdate();
            }

            _inventoryManager.TryUseItem(rune.Bag, rune.Slot, skipApplyingItemEffect: true);

            return (true, item);
        }

        private void Compose(Item item, Item recRune)
        {
            switch (recRune.Special)
            {
                case SpecialEffect.RecreationRune:
                    RandomCompose(item);
                    break;

                case SpecialEffect.RecreationRune_STR:
                    ComposeStr(item);
                    break;

                case SpecialEffect.RecreationRune_DEX:
                    ComposeDex(item);
                    break;

                case SpecialEffect.RecreationRune_REC:
                    ComposeRec(item);
                    break;

                case SpecialEffect.RecreationRune_INT:
                    ComposeInt(item);
                    break;

                case SpecialEffect.RecreationRune_WIS:
                    ComposeWis(item);
                    break;

                case SpecialEffect.RecreationRune_LUC:
                    ComposeLuc(item);
                    break;

                default:
                    break;
            }
        }

        private void ComposeStr(Item item)
        {
            if (item.ComposedStr == 0)
                return;

            item.ComposedStr = _random.Next(1, item.ReqWis + 1);
        }

        private void ComposeDex(Item item)
        {
            if (item.ComposedDex == 0)
                return;

            item.ComposedDex = _random.Next(1, item.ReqWis + 1);
        }

        private void ComposeRec(Item item)
        {
            if (item.ComposedRec == 0)
                return;

            item.ComposedRec = _random.Next(1, item.ReqWis + 1);
        }

        private void ComposeInt(Item item)
        {
            if (item.ComposedInt == 0)
                return;

            item.ComposedInt = _random.Next(1, item.ReqWis + 1);
        }

        private void ComposeWis(Item item)
        {
            if (item.ComposedWis == 0)
                return;

            item.ComposedWis = _random.Next(1, item.ReqWis + 1);
        }

        private void ComposeLuc(Item item)
        {
            if (item.ComposedLuc == 0)
                return;

            item.ComposedLuc = _random.Next(1, item.ReqWis + 1);
        }

        private void RandomCompose(Item item)
        {
            item.ComposedStr = 0;
            item.ComposedDex = 0;
            item.ComposedRec = 0;
            item.ComposedInt = 0;
            item.ComposedWis = 0;
            item.ComposedLuc = 0;
            item.ComposedHP = 0;
            item.ComposedMP = 0;
            item.ComposedSP = 0;

            var maxIndex = item.IsWeapon ? 6 : 9; // Weapons can not have hp, mp or sp recreated.
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
                        item.ComposedStr = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 1:
                        item.ComposedDex = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 2:
                        item.ComposedRec = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 3:
                        item.ComposedInt = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 4:
                        item.ComposedWis = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 5:
                        item.ComposedLuc = _random.Next(1, item.ReqWis + 1);
                        break;

                    case 6:
                        item.ComposedHP = _random.Next(1, item.ReqWis + 1) * 100;
                        break;

                    case 7:
                        item.ComposedMP = _random.Next(1, item.ReqWis + 1) * 100;
                        break;

                    case 8:
                        item.ComposedSP = _random.Next(1, item.ReqWis + 1) * 100;
                        break;

                    default:
                        break;
                }
            }
        }

        public (bool Success, Item Item) AbsoluteCompose(byte runeBag, byte runeSlot, byte itemBag, byte itemSlot)
        {
            _inventoryManager.InventoryItems.TryGetValue((runeBag, runeSlot), out var rune);
            _inventoryManager.InventoryItems.TryGetValue((itemBag, itemSlot), out var item);

            if (rune is null || item is null || rune.Special != SpecialEffect.AbsoluteRecreationRune || !item.IsComposable)
            {
                return (false, item);
            }

            var itemClone = item.Clone();
            Compose(itemClone, rune);

            // TODO: I'm not sure how absolute composite works and what to do next.

            return (true, itemClone);
        }

        public (bool Success, Item PerfectRune) TryRuneSynthesize(byte runeBag, byte runeSlot, byte vialBag, byte vialSlot)
        {
            if (_inventoryManager.IsFull)
                return (false, null);

            _inventoryManager.InventoryItems.TryGetValue((runeBag, runeSlot), out var rune);
            if (rune is null || rune.Special != SpecialEffect.RecreationRune || rune.Count < 2)
                return (false, null);

            _inventoryManager.InventoryItems.TryGetValue((vialBag, vialSlot), out var vial);
            if (vial is null || (vial.Special != SpecialEffect.RecreationRune_Vial_STR &&
                                 vial.Special != SpecialEffect.RecreationRune_Vial_DEX &&
                                 vial.Special != SpecialEffect.RecreationRune_Vial_INT &&
                                 vial.Special != SpecialEffect.RecreationRune_Vial_REC &&
                                 vial.Special != SpecialEffect.RecreationRune_Vial_WIS &&
                                 vial.Special != SpecialEffect.RecreationRune_Vial_LUC))
                return (false, null);

            _inventoryManager.TryUseItem(rune.Bag, rune.Slot, skipApplyingItemEffect: true, count: 2);
            _inventoryManager.TryUseItem(vial.Bag, vial.Slot, skipApplyingItemEffect: true);

            var perfectRune = new Item(_definitionsPreloader, _itemEnchantConfig, _itemCreateConfig, 101, (byte)(vial.Special - 92));
            _inventoryManager.AddItem(perfectRune, "rune_synthesize");

            return (true, perfectRune);
        }

        #endregion

        #region Enchantment

        public int GetEnchantmentRate(Item item, Item lapisia, Item rateBooster)
        {
            int rate = 0;

            if (item.EnchantmentLevel == 20)
                return rate;

            if (lapisia.EnchantRate > 0)
                return lapisia.EnchantRate * 100;

            var suffix = item.EnchantmentLevel > 9 ? $"{item.EnchantmentLevel}" : $"0{item.EnchantmentLevel}";

            if (item.IsWeapon)
                _itemEnchantConfig.LapisianEnchantPercentRate.TryGetValue($"WeaponStep{suffix}", out rate);
            else
                if (item.IsArmor)
                _itemEnchantConfig.LapisianEnchantPercentRate.TryGetValue($"DefenseStep{suffix}", out rate);

            if (rateBooster is not null)
                rate += rateBooster.LinkingRate * 100;

            return rate;
        }

        public uint GetEnchantmentGold(Item item)
        {
            return (uint)(item.IsArmor ? 1040000 : 2500000);
        }

        public (bool Success, Item Item, Item Lapisia, bool SafetyScrollLeft) TryEnchant(byte bag, byte slot, byte lapisiaBag, byte lapisiaSlot)
        {
            _inventoryManager.InventoryItems.TryGetValue((lapisiaBag, lapisiaSlot), out var lapisia);
            if (lapisia is null || (lapisia.Special != SpecialEffect.Lapisia && lapisia.Type != 95))
                return (false, null, null, false);

            _inventoryManager.InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null || item.EnchantmentLevel == 20)
                return (false, null, null, false);

            if ((item.IsWeapon && !lapisia.IsWeaponLapisia) || (item.IsArmor && !lapisia.IsArmorLapisia) || (item.IsShield && !lapisia.IsWeaponLapisia))
                return (false, null, null, false);

            if (item.EnchantmentLevel < lapisia.MinEnchantLevel || item.EnchantmentLevel >= lapisia.MaxEnchantLevel && lapisia.MinEnchantLevel != lapisia.MaxEnchantLevel)
                return (false, null, null, false);

            var neededGold = GetEnchantmentGold(item);
            if (_inventoryManager.Gold < neededGold)
                return (false, null, null, false);

            _inventoryManager.Gold -= neededGold;

            var usedSafetyScroll = false;
            var safetyScroll = _inventoryManager.InventoryItems.Values.FirstOrDefault(x => x.Special == SpecialEffect.SafetyEnchant);
            if (safetyScroll is not null)
            {
                _inventoryManager.TryUseItem(safetyScroll.Bag, safetyScroll.Slot, skipApplyingItemEffect: true);
                usedSafetyScroll = true;
            }

            var rateBooster = _inventoryManager.InventoryItems.Values.FirstOrDefault(x => x.Special == SpecialEffect.EnchantEnhancer);
            if (rateBooster is not null)
                _inventoryManager.TryUseItem(rateBooster.Bag, rateBooster.Slot, skipApplyingItemEffect: true);

            var ok = Enchant(item, lapisia, rateBooster);
            var oldLevel = item.EnchantmentLevel;

            var itemDestroyed = false;
            if (ok)
                item.EnchantmentLevel++;
            else if (!usedSafetyScroll)
            {
                if (lapisia.CanBreakItem)
                    itemDestroyed = true;
                else
                if (item.EnchantmentLevel != 0)
                    item.EnchantmentLevel--;
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

                    _inventoryManager.RemoveItem(item, "enchant_failed");
                }
                else
                {
                    var oldSuffix = oldLevel > 9 ? $"{oldLevel}" : $"0{oldLevel}";
                    var suffix = item.EnchantmentLevel > 9 ? $"{item.EnchantmentLevel}" : $"0{item.EnchantmentLevel}";
                    if (item.IsWeapon)
                    {
                        _statsManager.WeaponMinAttack -= _itemEnchantConfig.LapisianEnchantAddValue[$"WeaponStep{oldSuffix}"];
                        _statsManager.WeaponMaxAttack -= _itemEnchantConfig.LapisianEnchantAddValue[$"WeaponStep{oldSuffix}"];
                        _statsManager.WeaponMinAttack += _itemEnchantConfig.LapisianEnchantAddValue[$"WeaponStep{suffix}"];
                        _statsManager.WeaponMaxAttack += _itemEnchantConfig.LapisianEnchantAddValue[$"WeaponStep{suffix}"];
                        _statsManager.RaiseAdditionalStatsUpdate();
                    }
                    else
                    {
                        _statsManager.Absorption -= (ushort)_itemEnchantConfig.LapisianEnchantAddValue[$"DefenseStep{oldSuffix}"];
                        _statsManager.Absorption += (ushort)_itemEnchantConfig.LapisianEnchantAddValue[$"DefenseStep{suffix}"];
                    }

                    _inventoryManager.RaiseEquipmentChanged(item.Slot);
                }
            }

            return (ok, item, lapisia, safetyScroll is null ? false : safetyScroll.Count > 0);
        }

        private bool Enchant(Item item, Item lapisia, Item rateBooster)
        {
            double rate = GetEnchantmentRate(item, lapisia, rateBooster) / 10000;
            var rand = _random.Next(1, 101);
            var success = rate >= rand;

            lapisia.Count--;
            if (lapisia.Count == 0)
                _inventoryManager.RemoveItem(lapisia, "used_during_enchant");

            return success;
        }

        #endregion
    }
}
