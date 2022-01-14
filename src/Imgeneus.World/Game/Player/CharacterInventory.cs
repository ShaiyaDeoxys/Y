using Imgeneus.Database.Constants;
using Imgeneus.Database.Entities;
using Imgeneus.DatabaseBackgroundService.Handlers;
using Imgeneus.World.Game.Blessing;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.Skills;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Imgeneus.World.Game.Player
{
    public partial class Character
    {
        #region Inventory

        /// <summary>
        /// Collection of inventory items.
        /// </summary>
        public readonly ConcurrentDictionary<(byte Bag, byte Slot), Item> InventoryItems = new ConcurrentDictionary<(byte Bag, byte Slot), Item>();

        /// <summary>
        /// Adds item to player's inventory.
        /// </summary>
        /// <param name="itemType">item type</param>
        /// <param name="itemTypeId">item type id</param>
        /// <param name="count">how many items</param>
        public Item AddItemToInventory(Item item)
        {
            /*// Find free space.
            var free = FindFreeSlotInInventory();

            // Calculated bag slot can not be 0, because 0 means worn item. Newly created item can not be worn.
            if (free.Bag == 0 || free.Slot == -1)
            {
                return null;
            }

            item.Bag = free.Bag;
            item.Slot = (byte)free.Slot;

            _taskQueue.Enqueue(ActionType.SAVE_ITEM_IN_INVENTORY,
                               Id, item.Type, item.TypeId, item.Count, item.Quality, item.Bag, item.Slot,
                               item.Gem1 is null ? 0 : item.Gem1.TypeId,
                               item.Gem2 is null ? 0 : item.Gem2.TypeId,
                               item.Gem3 is null ? 0 : item.Gem3.TypeId,
                               item.Gem4 is null ? 0 : item.Gem4.TypeId,
                               item.Gem5 is null ? 0 : item.Gem5.TypeId,
                               item.Gem6 is null ? 0 : item.Gem6.TypeId,
                               item.DyeColor.IsEnabled, item.DyeColor.Alpha, item.DyeColor.Saturation, item.DyeColor.R, item.DyeColor.G, item.DyeColor.B, item.CreationTime, item.ExpirationTime);

            InventoryItems.TryAdd((item.Bag, item.Slot), item);

            if (item.ExpirationTime != null)
            {
                item.OnExpiration += CharacterItem_OnExpiration;
            }

            _logger.LogDebug($"Character {Id} got item {item.Type} {item.TypeId}");
            return item;*/
            return null;
        }

        /// <summary>
        /// Removes item from inventory
        /// </summary>
        /// <param name="item">item, that we want to remove</param>
        public Item RemoveItemFromInventory(Item item)
        {
            /*  // If we are giving consumable item.
              if (item.TradeQuantity < item.Count && item.TradeQuantity != 0)
              {
                  var givenItem = item.Clone();
                  givenItem.Count = item.TradeQuantity;

                  item.Count -= item.TradeQuantity;
                  item.TradeQuantity = 0;

                  _taskQueue.Enqueue(ActionType.UPDATE_ITEM_COUNT_IN_INVENTORY,
                                     Id, item.Bag, item.Slot, item.Count);

                  return givenItem;
              }

              _taskQueue.Enqueue(ActionType.REMOVE_ITEM_FROM_INVENTORY,
                                 Id, item.Bag, item.Slot);

              InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);

              if (item.ExpirationTime != null)
              {
                  item.StopExpirationTimer();
                  item.OnExpiration -= CharacterItem_OnExpiration;
              }

              _logger.LogDebug($"Character {Id} lost item {item.Type} {item.TypeId}");
              return item;*/
            return null;
        }

        #endregion

        #region Use Item

        /// <summary>
        /// Event, that is fired, when player uses any item from inventory.
        /// </summary>
        public event Action<Character, Item> OnUsedItem;

        /// <summary>
        /// Use item from inventory.
        /// </summary>
        /// <param name="bag">bag, where item is situated</param>
        /// <param name="slot">slot, where item is situated</param>
        /// <param name="targetId">id of another player</param>
        public void TryUseItem(byte bag, byte slot, int? targetId = null)
        {
            InventoryItems.TryGetValue((bag, slot), out var item);
            if (item is null)
            {
                _logger.LogWarning($"Character {Id} is trying to use item, that does not exist. Possible hack?");
                return;
            }

            if (!CanUseItem(item))
            {
                _packetsHelper.SendCanNotUseItem(Client, Id);
                return;
            }

            if (targetId != null)
            {
                if (!CanUseItemOnTarget(item, (int)targetId))
                {
                    _packetsHelper.SendCanNotUseItem(Client, Id);
                    return;
                }
            }

            item.Count--;
            _itemCooldowns[item.ReqIg] = DateTime.UtcNow;
            ApplyItemEffect(item, targetId);
            OnUsedItem?.Invoke(this, item);

            if (item.Count > 0)
            {
                _taskQueue.Enqueue(ActionType.UPDATE_ITEM_COUNT_IN_INVENTORY,
                                   Id, item.Bag, item.Slot, item.Count);
            }
            else
            {
                InventoryItems.TryRemove((item.Bag, item.Slot), out var removedItem);
                _taskQueue.Enqueue(ActionType.REMOVE_ITEM_FROM_INVENTORY,
                                   Id, item.Bag, item.Slot);
            }
        }

        /// <summary>
        /// Adds the effect of the item to the character.
        /// </summary>
        private void ApplyItemEffect(Item item, int? targetId = null)
        {
            switch (item.Special)
            {
                case SpecialEffect.None:
                    if (item.HP > 0 || item.MP > 0 || item.SP > 0)
                        UseHealingPotion(item);

                    if (item.SkillId != 0)
                        SkillsManager.UseSkill(new Skill(_databasePreloader.Skills[(item.SkillId, item.SkillLevel)], ISkillsManager.ITEM_SKILL_NUMBER, 0), this);

                    break;

                case SpecialEffect.PercentHealingPotion:
                    UsePercentHealingPotion(item);
                    break;

                case SpecialEffect.HypnosisCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    break;

                case SpecialEffect.StunCure:
                    UseCureDebuffPotion(StateType.Stun);
                    break;

                case SpecialEffect.SilenceCure:
                    UseCureDebuffPotion(StateType.Silence);
                    break;

                case SpecialEffect.DarknessCure:
                    UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.StopCure:
                    UseCureDebuffPotion(StateType.Immobilize);
                    break;

                case SpecialEffect.SlowCure:
                    UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.VenomCure:
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    break;

                case SpecialEffect.DiseaseCure:
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.IllnessDelusionCure:
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    break;

                case SpecialEffect.SleepStunStopSlowCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    UseCureDebuffPotion(StateType.Stun);
                    UseCureDebuffPotion(StateType.Immobilize);
                    UseCureDebuffPotion(StateType.Slow);
                    break;

                case SpecialEffect.SilenceDarknessCure:
                    UseCureDebuffPotion(StateType.Silence);
                    UseCureDebuffPotion(StateType.Darkness);
                    break;

                case SpecialEffect.DullBadLuckCure:
                    UseCureDebuffPotion(StateType.DexDecrease);
                    UseCureDebuffPotion(StateType.Misfortunate);
                    break;

                case SpecialEffect.DoomFearCure:
                    UseCureDebuffPotion(StateType.MentalSmasher);
                    UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.FullCure:
                    UseCureDebuffPotion(StateType.Sleep);
                    UseCureDebuffPotion(StateType.Stun);
                    UseCureDebuffPotion(StateType.Silence);
                    UseCureDebuffPotion(StateType.Darkness);
                    UseCureDebuffPotion(StateType.Immobilize);
                    UseCureDebuffPotion(StateType.Slow);
                    UseCureDebuffPotion(StateType.HPDamageOverTime);
                    UseCureDebuffPotion(StateType.SPDamageOverTime);
                    UseCureDebuffPotion(StateType.MPDamageOverTime);
                    UseCureDebuffPotion(StateType.DexDecrease);
                    UseCureDebuffPotion(StateType.Misfortunate);
                    UseCureDebuffPotion(StateType.MentalSmasher);
                    UseCureDebuffPotion(StateType.LowerAttackOrDefence);
                    break;

                case SpecialEffect.DisorderCure:
                    // ?
                    break;

                case SpecialEffect.StatResetStone:
                    ResetStats();
                    break;

                case SpecialEffect.GoddessBlessing:
                    UseBlessItem();
                    break;

                case SpecialEffect.AppearanceChange:
                case SpecialEffect.SexChange:
                    // Used in ChangeAppearance call.
                    break;

                case SpecialEffect.LinkingHammer:
                case SpecialEffect.PerfectLinkingHammer:
                case SpecialEffect.RecreationRune:
                case SpecialEffect.AbsoluteRecreationRune:
                    // Effect is added in linking manager.
                    break;

                case SpecialEffect.Dye:
                    // Effect is handled in dyeing manager.
                    break;

                case SpecialEffect.NameChange:
                    UseNameChangeStone();
                    break;

                case SpecialEffect.AnotherItemGenerator:
                    // TODO: Generate another item based on item ReqVg.
                    break;

                case SpecialEffect.SkillResetStone:
                    ResetSkills();
                    break;

                case SpecialEffect.MovementRune:
                    if (_gameWorld.Players.TryGetValue((int)targetId, out var target))
                        Teleport(target.Map.Id, target.PosX, target.PosY, target.PosZ);
                    break;

                default:
                    _logger.LogError($"Uninplemented item effect {item.Special}.");
                    break;
            }
        }

        /// <summary>
        /// Event, that is fired, when player changes appearance.
        /// </summary>
        public event Action<Character> OnAppearanceChanged;

        /// <summary>
        /// Changes player's appearance.
        /// </summary>
        /// <param name="face">new face</param>
        /// <param name="hair">new hair</param>
        /// <param name="size">new size</param>
        /// <param name="sex">new sex</param>
        private void ChangeAppearance(byte face, byte hair, byte size, byte sex)
        {
            Face = face;
            Hair = hair;
            Height = size;
            Gender = (Gender)sex;

            _taskQueue.Enqueue(ActionType.SAVE_APPEARANCE, Id, Face, Hair, Height, Gender);

            OnAppearanceChanged?.Invoke(this);
        }

        /// <summary>
        /// Initiates name change process
        /// </summary>
        public void UseNameChangeStone()
        {
            IsRename = true;

            _taskQueue.Enqueue(ActionType.SAVE_IS_RENAME, Id, true);
        }

        /// <summary>
        /// Uses potion, that restores hp,sp,mp.
        /// </summary>
        private void UseHealingPotion(Item potion)
        {
            HealthManager.Recover(potion.HP, potion.MP, potion.SP);
        }

        /// <summary>
        /// Uses potion, that restores % of hp,sp,mp.
        /// </summary>
        private void UsePercentHealingPotion(Item potion)
        {
            var hp = Convert.ToInt32(HealthManager.MaxHP * potion.HP / 100);
            var mp = Convert.ToInt32(HealthManager.MaxMP * potion.MP / 100);
            var sp = Convert.ToInt32(HealthManager.MaxSP * potion.SP / 100);

            HealthManager.Recover(hp, mp, sp);
        }

        /// <summary>
        /// Cures character from some debuff.
        /// </summary>
        private void UseCureDebuffPotion(StateType debuffType)
        {
            var debuffs = BuffsManager.ActiveBuffs.Where(b => b.StateType == debuffType).ToList();
            foreach (var d in debuffs)
            {
                d.CancelBuff();
            }
        }

        /// <summary>
        /// GM item ,that increases bless amount of player's fraction.
        /// </summary>
        private void UseBlessItem()
        {
            if (Country == Fraction.Light)
                Bless.Instance.LightAmount += 500;
            else
                Bless.Instance.DarkAmount += 500;
        }

        private readonly Dictionary<byte, DateTime> _itemCooldowns = new Dictionary<byte, DateTime>();

        /// <summary>
        /// Checks if item can be used. E.g. cooldown is over, required level is right etc.
        /// </summary>
        public bool CanUseItem(Item item)
        {
            if (item.Special == SpecialEffect.None && item.HP == 0 && item.MP == 0 && item.SP == 0 && item.SkillId == 0)
                return false;

            if (item.Type == Item.GEM_ITEM_TYPE)
                return true;

            if (item.ReqIg != 0)
            {
                if (_itemCooldowns.ContainsKey(item.ReqIg) && Item.ReqIgToCooldownInMilliseconds.ContainsKey(item.ReqIg))
                {
                    if (DateTime.UtcNow.Subtract(_itemCooldowns[item.ReqIg]).TotalMilliseconds < Item.ReqIgToCooldownInMilliseconds[item.ReqIg])
                        return false;
                }
            }

            if (item.Reqlevel > LevelProvider.Level)
                return false;

            if (LevelingManager.Grow < item.Grow)
                return false;

            switch (item.ItemClassType)
            {
                case ItemClassType.Human:
                    if (Race != Race.Human)
                        return false;
                    break;

                case ItemClassType.Elf:
                    if (Race != Race.Elf)
                        return false;
                    break;

                case ItemClassType.AllLights:
                    if (Country != Fraction.Light)
                        return false;
                    break;

                case ItemClassType.Deatheater:
                    if (Race != Race.DeathEater)
                        return false;
                    break;

                case ItemClassType.Vail:
                    if (Race != Race.Vail)
                        return false;
                    break;

                case ItemClassType.AllFury:
                    if (Country != Fraction.Dark)
                        return false;
                    break;
            }

            if (item.ItemClassType != ItemClassType.AllFactions)
            {
                switch (Class)
                {
                    case CharacterProfession.Fighter:
                        if (!item.IsForFighter)
                            return false;
                        break;

                    case CharacterProfession.Defender:
                        if (!item.IsForDefender)
                            return false;
                        break;

                    case CharacterProfession.Ranger:
                        if (!item.IsForRanger)
                            return false;
                        break;

                    case CharacterProfession.Archer:
                        if (!item.IsForArcher)
                            return false;
                        break;

                    case CharacterProfession.Mage:
                        if (!item.IsForMage)
                            return false;
                        break;

                    case CharacterProfession.Priest:
                        if (!item.IsForPriest)
                            return false;
                        break;
                }
            }

            switch (item.Special)
            {
                case SpecialEffect.RecreationRune:
                case SpecialEffect.AbsoluteRecreationRune:
                case SpecialEffect.RecreationRune_STR:
                case SpecialEffect.RecreationRune_DEX:
                case SpecialEffect.RecreationRune_REC:
                case SpecialEffect.RecreationRune_INT:
                case SpecialEffect.RecreationRune_WIS:
                case SpecialEffect.RecreationRune_LUC:
                    return _linkingManager.Item != null && _linkingManager.Item.IsComposable;
            }

            return true;
        }

        /// <summary>
        /// Checks if item can be used on another player.
        /// </summary>
        public bool CanUseItemOnTarget(Item item, int targetId)
        {
            switch (item.Special)
            {
                case SpecialEffect.MovementRune:
                    if (_gameWorld.Players.TryGetValue(targetId, out var target))
                    {
                        if (target.Party != Party)
                            return false;

                        return _gameWorld.CanTeleport(this, target.MapId, out var reason);
                    }
                    else
                        return false;

                default:
                    return true;
            }
        }

        #endregion

        #region Buy/sell Item

        /// <summary>
        /// Buys item from npc store.
        /// </summary>
        /// <param name="product">product to buy</param>
        /// <param name="count">how many items player want to buy</param>
        public Item BuyItem(NpcProduct product, byte count)
        {
            _databasePreloader.Items.TryGetValue((product.Type, product.TypeId), out var dbItem);
            if (dbItem is null)
            {
                _logger.LogError($"Trying to buy not presented item(type={product.Type},typeId={product.TypeId}).");
                return null;
            }

            if (dbItem.Buy * count > InventoryManager.Gold) // Not enough money.
            {
                _packetsHelper.SendBuyItemIssue(Client, 1);
                return null;
            }

            var freeSlot = FindFreeSlotInInventory();
            if (freeSlot.Slot == -1) // No free slot.
            {
                _packetsHelper.SendBuyItemIssue(Client, 2);
                return null;
            }

            InventoryManager.Gold = (uint)(InventoryManager.Gold - dbItem.Buy * count);
            var item = new Item(_databasePreloader, dbItem.Type, dbItem.TypeId);
            item.Count = count;

            return AddItemToInventory(item);
        }

        /// <summary>
        /// Sells item.
        /// </summary>
        /// <param name="item">item to sell</param>
        /// <param name="count">how many item player want to sell</param>
        public Item SellItem(Item item, byte count)
        {
            if (!InventoryItems.ContainsKey((item.Bag, item.Slot)))
            {
                return null;
            }

            item.TradeQuantity = count > item.Count ? item.Count : count;
            InventoryManager.Gold = (uint)(InventoryManager.Gold + item.Sell * item.TradeQuantity);
            return RemoveItemFromInventory(item);
        }

        #endregion

        #region Item Expiration

        public void CharacterItem_OnExpiration(Item item)
        {
            RemoveItemFromInventory(item);
            SendRemoveItemFromInventory(item, true);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Tries to find free slot in inventory.
        /// </summary>
        /// <returns>tuple of bag and slot; slot is -1 if there is no free slot</returns>
        private (byte Bag, int Slot) FindFreeSlotInInventory()
        {
            byte bagSlot = 0;
            int freeSlot = -1;

            if (InventoryItems.Count > 0)
            {
                var maxBag = 5;
                var maxSlots = 24;

                // Go though all bags and try to find any free slot.
                // Start with 1, because 0 is worn items.
                for (byte i = 1; i <= maxBag; i++)
                {
                    var bagItems = InventoryItems.Where(itm => itm.Value.Bag == i).OrderBy(b => b.Value.Slot);
                    for (var j = 0; j < maxSlots; j++)
                    {
                        if (!bagItems.Any(b => b.Value.Slot == j))
                        {
                            freeSlot = j;
                            break;
                        }
                    }

                    if (freeSlot != -1)
                    {
                        bagSlot = i;
                        break;
                    }
                }
            }
            else
            {
                bagSlot = 1; // Start with 1, because 0 is worn items.
                freeSlot = 0;
            }

            return (bagSlot, freeSlot);
        }


        #endregion
    }
}
