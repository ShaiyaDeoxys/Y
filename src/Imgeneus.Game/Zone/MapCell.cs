using Imgeneus.Core.Extensions;
using Imgeneus.Database.Constants;
using Imgeneus.Game.Skills;
using Imgeneus.World.Game.Attack;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Health;
using Imgeneus.World.Game.Inventory;
using Imgeneus.World.Game.Monster;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.NPCs;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Player;
using Imgeneus.World.Game.Shape;
using Imgeneus.World.Game.Speed;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imgeneus.World.Game.Zone
{
    public class MapCell : IDisposable
    {
        public MapCell(int index, IEnumerable<int> neighborCells, Map map)
        {
            CellIndex = index;
            NeighborCells = neighborCells;
            Map = map;
        }

        public int CellIndex { get; private set; }

        public IEnumerable<int> NeighborCells { get; private set; }

        protected Map Map { get; private set; }

        /// <summary>
        /// Sets cell index to each cell member.
        /// </summary>
        private void AssignCellIndex(IMapMember member)
        {
            member.OldCellId = member.CellId;
            member.CellId = CellIndex;
        }

        #region Players

        /// <summary>
        /// Thread-safe dictionary of connected players. Key is character id, value is character.
        /// </summary>
        private readonly ConcurrentDictionary<int, Character> Players = new ConcurrentDictionary<int, Character>();

        /// <summary>
        /// Adds character to map cell.
        /// </summary>
        public void AddPlayer(Character character)
        {
            Players.TryAdd(character.Id, character);
            AddListeners(character);
            AssignCellIndex(character);

            // Send update players.
            var oldPlayers = character.OldCellId != -1 ? Map.Cells[character.OldCellId].GetAllPlayers(true) : new List<Character>();
            var newPlayers = GetAllPlayers(true);

            var sendPlayerLeave = oldPlayers.Where(p => !newPlayers.Contains(p) && p != character);
            var sendPlayerEnter = newPlayers.Where(p => !oldPlayers.Contains(p));

            foreach (var player in sendPlayerLeave)
            {
                Map.PacketFactory.SendCharacterLeave(player.GameSession.Client, character);
                Map.PacketFactory.SendCharacterLeave(character.GameSession.Client, player);
            }

            foreach (var player in sendPlayerEnter)
                if (player.Id != character.Id)
                {
                    // Notify players in this map, that new player arrived.
                    Map.PacketFactory.SendCharacterEnter(player.GameSession.Client, character);

                    // Notify new player, about already loaded player.
                    Map.PacketFactory.SendCharacterEnter(character.GameSession.Client, player);
                    if (player.ShopManager.IsShopOpened)
                        Map.PacketFactory.SendMyShopStarted(character.GameSession.Client, player.Id, player.ShopManager.Name);
                    if (player.ShapeManager.IsTranformated)
                        Map.PacketFactory.SendTransformation(character.GameSession.Client, player.Id, player.ShapeManager.IsTranformated);
                }
                else // Original server sends this also to player himself, although I'm not sure if it's needed.
                     // Added it as a fix for admin stealth.
                    if (character.OldCellId == -1)
                    Map.PacketFactory.SendCharacterEnter(character.GameSession.Client, character);

            // Send update npcs.
            var oldCellNPCs = character.OldCellId != -1 ? Map.Cells[character.OldCellId].GetAllNPCs(true) : new List<Npc>();
            var newCellNPCs = GetAllNPCs(true);

            var npcToLeave = oldCellNPCs.Where(npc => !newCellNPCs.Contains(npc));
            var npcToEnter = newCellNPCs.Where(npc => !oldCellNPCs.Contains(npc));

            foreach (var npc in npcToLeave)
                Map.PacketFactory.SendNpcLeave(character.GameSession.Client, npc);
            foreach (var npc in npcToEnter)
                Map.PacketFactory.SendNpcEnter(character.GameSession.Client, npc);

            // Send update mobs.
            var oldCellMobs = character.OldCellId != -1 ? Map.Cells[character.OldCellId].GetAllMobs(true) : new List<Mob>();
            var newCellMobs = GetAllMobs(true);

            var mobToLeave = oldCellMobs.Where(m => !newCellMobs.Contains(m));
            var mobToEnter = newCellMobs.Where(m => !oldCellMobs.Contains(m));

            foreach (var mob in mobToLeave)
                Map.PacketFactory.SendMobLeave(character.GameSession.Client, mob);

            foreach (var mob in mobToEnter)
                Map.PacketFactory.SendMobEnter(character.GameSession.Client, mob, false);
        }

        /// <summary>
        /// Tries to get player from map cell.
        /// </summary>
        /// <param name="playerId">id of player, that you are trying to get.</param>
        /// <returns>either player or null if player is not presented</returns>
        public Character GetPlayer(int playerId)
        {
            Players.TryGetValue(playerId, out var player);
            return player;
        }

        /// <summary>
        /// Gets all players from map cell.
        /// </summary>
        /// <param name="includeNeighborCells">if set to true includes characters fom neighbor cells</param>
        public IEnumerable<Character> GetAllPlayers(bool includeNeighborCells)
        {
            var myPlayers = Players.Values;
            if (includeNeighborCells)
                return myPlayers.Concat(NeighborCells.Select(index => Map.Cells[index]).SelectMany(cell => cell.GetAllPlayers(false))).Distinct();
            return myPlayers;
        }

        /// <summary>
        /// Gets player near point.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="z">z coordinate</param>
        /// <param name="range">minimum range to target, if set to 0 is not calculated</param>
        /// <param name="country">light, dark or both</param>
        /// <param name="includeDead">include dead players or not</param>
        /// <param name="includeNeighborCells">include players from neighbor cells, usually true</param>
        public IEnumerable<IKillable> GetPlayers(float x, float z, byte range, CountryType country = CountryType.None, bool includeDead = false, bool includeNeighborCells = true)
        {
            var myPlayers = Players.Values.Where(
                     p => (includeDead || !p.HealthManager.IsDead) && // filter by death
                     (p.CountryProvider.Country == country || country == CountryType.None) && // filter by fraction
                     (range == 0 || MathExtensions.Distance(x, p.PosX, z, p.PosZ) <= range)); // filter by range
            if (includeNeighborCells)
                return myPlayers.Concat(NeighborCells.Select(index => Map.Cells[index]).SelectMany(cell => cell.GetPlayers(x, z, range, country, includeDead, false))).Distinct();
            return myPlayers;
        }

        /// <summary>
        /// Gets enemies near target.
        /// </summary>
        public IEnumerable<IKillable> GetEnemies(IKiller sender, float x, float z, byte range)
        {
            IEnumerable<IKillable> mobs = sender is Mob ? new List<IKillable>() : GetAllMobs(true).Where(m => !m.HealthManager.IsDead && m.HealthManager.IsAttackable && m.CountryProvider.Country != sender.CountryProvider.Country && MathExtensions.Distance(x, m.PosX, z, m.PosZ) <= range);
            IEnumerable<IKillable> chars = GetAllPlayers(true).Where(p => !p.HealthManager.IsDead && p.HealthManager.IsAttackable && p.CountryProvider.Country != sender.CountryProvider.Country && MathExtensions.Distance(x, p.PosX, z, p.PosZ) <= range);

            return mobs.Concat(chars);
        }

        /// <summary>
        /// Removes player from map cell.
        /// </summary>
        public void RemovePlayer(Character character, bool notifyPlayers)
        {
            RemoveListeners(character);
            Players.TryRemove(character.Id, out var removedCharacter);

            foreach (var mob in GetAllMobs(true).Where(m => m.AttackManager.Target == character))
                mob.AttackManager.Target = null;

            if (notifyPlayers)
                foreach (var player in GetAllPlayers(true))
                    Map.PacketFactory.SendCharacterLeave(player.GameSession.Client, character);
        }

        /// <summary>
        /// Subscribes to character events.
        /// </summary>
        private void AddListeners(Character character)
        {
            // Map with id is test map.
            if (character.MapProvider.NextMapId == Map.TEST_MAP_ID)
                return;
            character.MovementManager.OnMove += Character_OnMove;
            character.OnMotion += Character_OnMotion;
            character.InventoryManager.OnEquipmentChanged += Character_OnEquipmentChanged;
            character.PartyManager.OnPartyChanged += Character_OnPartyChanged;
            character.SpeedManager.OnAttackOrMoveChanged += Character_OnAttackOrMoveChanged;
            character.SkillsManager.OnUsedSkill += Character_OnUsedSkill;
            character.SkillsManager.OnUsedRangeSkill += Character_OnUsedRangeSkill;
            character.AttackManager.OnAttack += Character_OnAttack;
            character.HealthManager.OnDead += Character_OnDead;
            character.SkillCastingManager.OnSkillCastStarted += Character_OnSkillCastStarted;
            character.InventoryManager.OnUsedItem += Character_OnUsedItem;
            character.HealthManager.OnMaxHPChanged += Character_OnMaxHPChanged;
            character.HealthManager.OnMaxSPChanged += Character_OnMaxSPChanged;
            character.HealthManager.OnMaxMPChanged += Character_OnMaxMPChanged;
            character.HealthManager.OnMirrowDamage += Character_OnMirrowDamage;
            character.HealthManager.OnRecover += Character_OnRecover;
            character.BuffsManager.OnSkillKeep += Character_OnSkillKeep;
            character.ShapeManager.OnShapeChange += Character_OnShapeChange;
            character.ShapeManager.OnTranformated += Character_OnTranformated;
            character.HealthManager.OnRebirthed += Character_OnRebirthed;
            character.AdditionalInfoManager.OnAppearanceChanged += Character_OnAppearanceChanged;
            character.VehicleManager.OnStartSummonVehicle += Character_OnStartSummonVehicle;
            character.LevelProvider.OnLevelUp += Character_OnLevelUp;
            character.VehicleManager.OnVehiclePassengerChanged += Character_OnVehiclePassengerChanged;
            character.TeleportationManager.OnTeleporting += Character_OnTeleport;
            character.PartyManager.OnSummonning += Character_OnItemCasting;
            character.TeleportationManager.OnCastingTeleport += Character_OnItemCasting;
            character.ShopManager.OnShopStarted += Character_OnShopStarted;
            character.ShopManager.OnShopFinished += Character_OnShopFinished;
        }

        /// <summary>
        /// Unsubscribes from character events.
        /// </summary>
        private void RemoveListeners(Character character)
        {
            character.MovementManager.OnMove -= Character_OnMove;
            character.OnMotion -= Character_OnMotion;
            character.InventoryManager.OnEquipmentChanged -= Character_OnEquipmentChanged;
            character.PartyManager.OnPartyChanged -= Character_OnPartyChanged;
            character.SpeedManager.OnAttackOrMoveChanged -= Character_OnAttackOrMoveChanged;
            character.SkillsManager.OnUsedSkill -= Character_OnUsedSkill;
            character.SkillsManager.OnUsedRangeSkill -= Character_OnUsedRangeSkill;
            character.AttackManager.OnAttack -= Character_OnAttack;
            character.HealthManager.OnDead -= Character_OnDead;
            character.SkillCastingManager.OnSkillCastStarted -= Character_OnSkillCastStarted;
            character.InventoryManager.OnUsedItem -= Character_OnUsedItem;
            character.HealthManager.OnMaxHPChanged -= Character_OnMaxHPChanged;
            character.HealthManager.OnMaxSPChanged -= Character_OnMaxSPChanged;
            character.HealthManager.OnMaxMPChanged -= Character_OnMaxMPChanged;
            character.HealthManager.OnMirrowDamage -= Character_OnMirrowDamage;
            character.HealthManager.OnRecover -= Character_OnRecover;
            character.BuffsManager.OnSkillKeep -= Character_OnSkillKeep;
            character.ShapeManager.OnShapeChange -= Character_OnShapeChange;
            character.ShapeManager.OnTranformated -= Character_OnTranformated;
            character.HealthManager.OnRebirthed -= Character_OnRebirthed;
            character.AdditionalInfoManager.OnAppearanceChanged -= Character_OnAppearanceChanged;
            character.VehicleManager.OnStartSummonVehicle -= Character_OnStartSummonVehicle;
            character.LevelProvider.OnLevelUp -= Character_OnLevelUp;
            character.VehicleManager.OnVehiclePassengerChanged -= Character_OnVehiclePassengerChanged;
            character.TeleportationManager.OnTeleporting -= Character_OnTeleport;
            character.PartyManager.OnSummonning -= Character_OnItemCasting;
            character.TeleportationManager.OnCastingTeleport -= Character_OnItemCasting;
            character.ShopManager.OnShopStarted -= Character_OnShopStarted;
            character.ShopManager.OnShopFinished -= Character_OnShopFinished;
        }

        #region Character listeners

        /// <summary>
        /// Notifies other players about position change.
        /// </summary>
        private void Character_OnMove(int senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            // Send other clients notification, that user is moving.
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendCharacterMoves(player.GameSession.Client, senderId, x, y, z, a, motion);
        }

        /// <summary>
        /// When player sends motion, we should resend this motion to all other players on this map.
        /// </summary>
        private void Character_OnMotion(Character playerWithMotion, Motion motion)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendCharacterMotion(player.GameSession.Client, playerWithMotion.Id, motion);
        }

        /// <summary>
        /// Notifies other players, that this player changed equipment.
        /// </summary>
        /// <param name="characterId">player, that changed equipment</param>
        /// <param name="equipmentItem">item, that was worn</param>
        /// <param name="slot">item slot</param>
        private void Character_OnEquipmentChanged(int characterId, Item equipmentItem, byte slot)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendCharacterChangedEquipment(player.GameSession.Client, characterId, equipmentItem, slot);
        }

        /// <summary>
        ///  Notifies other players, that player entered/left party or got/removed leader.
        /// </summary>
        private void Character_OnPartyChanged(Character sender)
        {
            foreach (var player in GetAllPlayers(true))
            {
                PartyMemberType type = PartyMemberType.NoParty;

                if (sender.PartyManager.IsPartyLead)
                    type = PartyMemberType.Leader;
                else if (sender.PartyManager.HasParty)
                    type = PartyMemberType.Member;

                Map.PacketFactory.SendCharacterPartyChanged(player.GameSession.Client, sender.Id, type);
            }
        }

        /// <summary>
        /// Notifies other players, that player changed attack/move speed.
        /// </summary>
        private void Character_OnAttackOrMoveChanged(int senderId, AttackSpeed attack, MoveSpeed move)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendAttackAndMovementSpeed(player.GameSession.Client, senderId, attack, move);
        }

        /// <summary>
        /// Notifies other players, that player used skill.
        /// </summary>
        private void Character_OnUsedSkill(int senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendCharacterUsedSkill(player.GameSession.Client, senderId, target, skill, attackResult);

                if (attackResult.Absorb != 0 && player == target)
                    Map.PacketFactory.SendAbsorbValue(player.GameSession.Client, attackResult.Absorb);
            }
        }

        /// <summary>
        /// Notifies other players, that player used auto attack.
        /// </summary>
        private void Character_OnAttack(int senderId, IKillable target, AttackResult attackResult)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendCharacterUsualAttack(player.GameSession.Client, senderId, target, attackResult);

                if (attackResult.Absorb != 0 && player == target)
                    Map.PacketFactory.SendAbsorbValue(player.GameSession.Client, attackResult.Absorb);
            }
        }

        /// <summary>
        /// Notifies other players, that player is dead.
        /// </summary>
        private void Character_OnDead(int senderId, IKiller killer)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendCharacterKilled(player.GameSession.Client, senderId, killer);

            if (killer is Character killerCharacter)
            {
                var killed = Players[senderId];
                var items = killed.GenerateDrop(killerCharacter);
                foreach (var item in items)
                    Map.AddItem(new MapItem(item, killerCharacter, killed.PosX, 0, killed.PosZ));
            }
        }

        /// <summary>
        /// Notifies other players, that player starts casting.
        /// </summary>
        private void Character_OnSkillCastStarted(int senderId, IKillable target, Skill skill)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendSkillCastStarted(player.GameSession.Client, senderId, target, skill);
        }

        /// <summary>
        /// Notifies other players, that player used some item.
        /// </summary>
        private void Character_OnUsedItem(int senderId, Item item)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendUsedItem(player.GameSession.Client, senderId, item);
        }

        private void Character_OnRecover(int senderId, int hp, int mp, int sp)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendRecoverCharacter(player.GameSession.Client, senderId, hp, mp, sp);
        }

        private void Character_OnMaxHPChanged(int senderId, int value)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMaxHitpoints(player.GameSession.Client, senderId, HitpointType.HP, value);
        }

        private void Character_OnMaxSPChanged(int senderId, int value)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMaxHitpoints(player.GameSession.Client, senderId, HitpointType.SP, value);
        }

        private void Character_OnMaxMPChanged(int senderId, int value)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMaxHitpoints(player.GameSession.Client, senderId, HitpointType.MP, value);
        }

        private void Character_OnMirrowDamage(int senderId, int targetId, Damage damage)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendCharacterMirrorDamage(player.GameSession.Client, senderId, targetId, damage);
        }

        private void Character_OnSkillKeep(int senderId, Buff buff, AttackResult result)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendSkillKeep(player.GameSession.Client, senderId, buff.SkillId, buff.SkillLevel, result);
        }

        private void Character_OnShapeChange(int senderId, ShapeEnum shape, int param1, int param2)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendShapeUpdate(player.GameSession.Client, senderId, shape, param1, param2);
        }

        private void Character_OnUsedRangeSkill(int senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendUsedRangeSkill(player.GameSession.Client, senderId, target, skill, attackResult);

                if (attackResult.Absorb != 0 && player == target)
                    Map.PacketFactory.SendAbsorbValue(player.GameSession.Client, attackResult.Absorb);
            }
        }

        private void Character_OnRebirthed(int senderId)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendCharacterRebirth(player.GameSession.Client, senderId);
                Map.PacketFactory.SendDeadRebirth(player.GameSession.Client, Players[senderId]);
            }
        }

        private void Character_OnAppearanceChanged(int characterId, byte hair, byte face, byte size, byte gender)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendAppearanceChanged(player.GameSession.Client, characterId, hair, face, size, gender);
        }

        private void Character_OnStartSummonVehicle(int senderId)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendStartSummoningVehicle(player.GameSession.Client, senderId);
        }

        /// <summary>
        /// Notifies other players that player levelled up
        /// </summary>
        private void Character_OnLevelUp(int senderId, ushort level, ushort oldLevel)
        {
            var sender = Players[senderId];

            foreach (var player in GetAllPlayers(true))
                // If sender has party, send admin level up
                Map.PacketFactory.SendLevelUp(player.GameSession.Client, senderId, level, sender.StatsManager.StatPoint, sender.SkillsManager.SkillPoints, sender.LevelingManager.MinLevelExp, sender.LevelingManager.NextLevelExp, sender.PartyManager.HasParty);
        }

        /// <summary>
        /// Notifies other players that 2 character now move together on 1 vehicle.
        /// </summary>
        private void Character_OnVehiclePassengerChanged(int senderId, int vehicle2CharacterID)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendVehiclePassengerChanged(player.GameSession.Client, senderId, vehicle2CharacterID);
        }

        /// <summary>
        /// Teleports player to new position.
        /// </summary>
        private void Character_OnTeleport(int senderId, ushort mapId, float x, float y, float z, bool teleportedByAdmin, bool summonedByAdmin)
        {
            foreach (var p in GetAllPlayers(true))
                if (!summonedByAdmin)
                    Map.PacketFactory.SendCharacterTeleport(p.GameSession.Client, senderId, mapId, x, y, z, teleportedByAdmin);
                else
                    Map.PacketFactory.SendGmSummon(p.GameSession.Client, senderId, mapId, x, y, z);
        }

        /// <summary>
        /// Notifies other players that character started casting some item.
        /// </summary>
        private void Character_OnItemCasting(int senderId)
        {
            foreach (var p in GetAllPlayers(true))
                Map.PacketFactory.SendItemCasting(p.GameSession.Client, senderId);
        }

        /// <summary>
        /// Notifies other players that character started shop.
        /// </summary>
        private void Character_OnShopStarted(int senderId, string shopName)
        {
            foreach (var p in GetAllPlayers(true))
                Map.PacketFactory.SendMyShopStarted(p.GameSession.Client, senderId, shopName);
        }

        /// <summary>
        /// Notifies other players that character closed shop.
        /// </summary>
        private void Character_OnShopFinished(int senderId)
        {
            foreach (var p in GetAllPlayers(true))
                Map.PacketFactory.SendMyShopFinished(p.GameSession.Client, senderId);
        }

        private void Character_OnTranformated(int senderId, bool isTransformed)
        {
            foreach (var p in GetAllPlayers(true))
                Map.PacketFactory.SendTransformation(p.GameSession.Client, senderId, isTransformed);
        }

        #endregion

        #endregion

        #region Mobs

        /// <summary>
        /// Thread-safe dictionary of monsters loaded to this map. Where key id mob id.
        /// </summary>
        private readonly ConcurrentDictionary<int, Mob> Mobs = new ConcurrentDictionary<int, Mob>();

        /// <summary>
        /// Adds mob to cell.
        /// </summary>
        public void AddMob(Mob mob)
        {
            Mobs.TryAdd(mob.Id, mob);
            AssignCellIndex(mob);
            AddListeners(mob);

            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobEnter(player.GameSession.Client, mob, true);
        }

        /// <summary>
        /// Removes mob from cell.
        /// </summary>
        public void RemoveMob(Mob mob)
        {
            Mobs.TryRemove(mob.Id, out var removedMob);
            RemoveListeners(removedMob);

            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobLeave(player.GameSession.Client, mob);
        }

        /// <summary>
        /// Tries to get mob from map.
        /// </summary>
        /// <param name="mobId">id of mob, that you are trying to get.</param>
        /// <param name="includeNeighborCells">search also in neighbor cells</param>
        /// <returns>either mob or null if mob is not presented</returns>
        public Mob GetMob(int mobId, bool includeNeighborCells)
        {
            Mob mob;
            Mobs.TryGetValue(mobId, out mob);

            if (mob is null && includeNeighborCells) // Maybe mob in neighbor cell?
                foreach (var cellId in NeighborCells)
                {
                    mob = Map.Cells[cellId].GetMob(mobId, false);
                    if (mob != null)
                        break;
                }

            return mob;
        }

        /// <summary>
        /// Called, when mob respawns.
        /// </summary>
        /// <param name="sender">respawned mob</param>
        public void RebirthMob(Mob sender)
        {
            sender.TimeToRebirth -= RebirthMob;

            // Create mob clone, because we can not reuse the same id.
            var mob = sender.Clone();

            // TODO: generate rebirth coordinates based on the spawn area.
            mob.MovementManager.PosX = sender.PosX;
            mob.MovementManager.PosY = sender.PosY;
            mob.MovementManager.PosZ = sender.PosZ;

            mob.HealthManager.Rebirth();

            AddMob(mob);
        }

        /// <summary>
        /// Gets all mobs from map cell.
        /// </summary>
        /// /// <param name="includeNeighborCells">if set to true includes mobs fom neighbor cells</param>
        public IEnumerable<Mob> GetAllMobs(bool includeNeighborCells)
        {
            var myMobs = Mobs.Values;
            if (includeNeighborCells)
                return myMobs.Concat(NeighborCells.Select(index => Map.Cells[index]).SelectMany(cell => cell.GetAllMobs(false))).Distinct();
            return myMobs;
        }

        /// <summary>
        /// Adds listeners to mob events.
        /// </summary>
        /// <param name="mob">mob, that we listen</param>
        private void AddListeners(Mob mob)
        {
            mob.HealthManager.OnDead += Mob_OnDead;
            mob.MovementManager.OnMove += Mob_OnMove;
            mob.AttackManager.OnAttack += Mob_OnAttack;
            mob.SkillsManager.OnUsedSkill += Mob_OnUsedSkill;
            mob.HealthManager.OnRecover += Mob_OnRecover;
            mob.BuffsManager.OnSkillKeep += Mob_OnSkillKeep;
            mob.HealthManager.OnMirrowDamage += Mob_OnMirrowDamage;
        }

        /// <summary>
        /// Removes listeners from mob.
        /// </summary>
        /// <param name="mob">mob, that we listen</param>
        private void RemoveListeners(Mob mob)
        {
            mob.HealthManager.OnDead -= Mob_OnDead;
            mob.MovementManager.OnMove -= Mob_OnMove;
            mob.AttackManager.OnAttack -= Mob_OnAttack;
            mob.SkillsManager.OnUsedSkill -= Mob_OnUsedSkill;
            mob.HealthManager.OnRecover -= Mob_OnRecover;
            mob.BuffsManager.OnSkillKeep -= Mob_OnSkillKeep;
            mob.HealthManager.OnMirrowDamage -= Mob_OnMirrowDamage;
            mob.TimeToRebirth -= RebirthMob;
        }

        private async void Mob_OnDead(int senderId, IKiller killer)
        {
            Mobs.TryRemove(senderId, out var mob);
            if (mob is null)
                return;

            RemoveListeners(mob);

            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobDead(player.GameSession.Client, senderId, killer);

            // Add experience to killer character/party
            // Update quest.
            if (killer is Character killerCharacter)
            {
                // For some unknown reason, when character gets new level, mob death animation is not played.
                // This is not pretty fix, but I could not find any reason why animation is not played.
                if (mob.Map.Id != Map.TEST_MAP_ID)
                    await Task.Delay(1000).ConfigureAwait(false);

                killerCharacter.LevelingManager.AddMobExperience(mob.LevelProvider.Level, (ushort)mob.Exp);
                killerCharacter.QuestsManager.UpdateQuestMobCount(mob.MobId);

                var items = mob.GenerateDrop(killerCharacter);
                if (killerCharacter.PartyManager.Party is null)
                {
                    foreach (var item in items)
                        Map.AddItem(new MapItem(item, killerCharacter, mob.PosX, 0, mob.PosZ));
                }
                else
                {
                    var notDistributedItems = killerCharacter.PartyManager.Party.DistributeDrop(items, killerCharacter);
                    foreach (var item in notDistributedItems)
                        Map.AddItem(new MapItem(item, killerCharacter, mob.PosX, 0, mob.PosZ));
                }
            }

            if (Map is GRBMap)
                (Map as GRBMap).AddPoints(mob.GuildPoints);

            if (mob.ShouldRebirth)
                mob.TimeToRebirth += RebirthMob;

            mob.Dispose();

#if DEBUG
            // ONLY for debug! Don't uncomment it in prod.
            // It's for checking if every scoped service is finalized, when mob is killed.

            // Wait 0.5 sec after client disconnected.
            await Task.Delay(500).ContinueWith((x) =>
            {
                // Collect everything.
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.WaitForFullGCComplete();
            });
#endif
        }

        private void Mob_OnMove(int senderId, float x, float y, float z, ushort a, MoveMotion motion)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobMove(player.GameSession.Client, senderId, x, z, motion);
        }

        private void Mob_OnAttack(int senderId, IKillable target, AttackResult attackResult)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendMobAttack(player.GameSession.Client, senderId, target.Id, attackResult);

                if (attackResult.Absorb != 0 && player == target)
                    Map.PacketFactory.SendAbsorbValue(player.GameSession.Client, attackResult.Absorb);
            }
        }

        private void Mob_OnUsedSkill(int senderId, IKillable target, Skill skill, AttackResult attackResult)
        {
            foreach (var player in GetAllPlayers(true))
            {
                Map.PacketFactory.SendMobUsedSkill(player.GameSession.Client, senderId, target.Id, skill, attackResult);

                if (attackResult.Absorb != 0 && player == target)
                    Map.PacketFactory.SendAbsorbValue(player.GameSession.Client, attackResult.Absorb);
            }
        }

        private void Mob_OnRecover(int senderId, int hp, int mp, int sp)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobRecover(player.GameSession.Client, senderId, hp);
        }

        private void Mob_OnSkillKeep(int senderId, Buff buff, AttackResult result)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobSkillKeep(player.GameSession.Client, senderId, buff.SkillId, buff.SkillLevel, result);
        }

        private void Mob_OnMirrowDamage(int senderId, int targetId, Damage damage)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendMobMirrorDamage(player.GameSession.Client, senderId, targetId, damage);
        }

        #endregion

        #region NPCs

        /// <summary>
        /// Thread-safe dictionary of npcs. Key is npc id, value is npc.
        /// </summary>
        private readonly ConcurrentDictionary<int, Npc> NPCs = new ConcurrentDictionary<int, Npc>();

        /// <summary>
        /// Adds npc to cell.
        /// </summary>
        /// <param name="npc">npc to add</param>
        public void AddNPC(Npc npc)
        {
            if (NPCs.TryAdd(npc.Id, npc))
            {
                AssignCellIndex(npc);
                AddListeners(npc);

                foreach (var player in GetAllPlayers(true))
                    Map.PacketFactory.SendNpcEnter(player.GameSession.Client, npc);
            }
        }

        /// <summary>
        /// Removes npc from cell.
        /// </summary>
        public void RemoveNPC(NpcType type, ushort typeId, byte count)
        {
            var npcs = NPCs.Values.Where(n => n.Type == type && n.TypeId == typeId).Take(count);
            foreach (var npc in npcs)
            {
                if (NPCs.TryRemove(npc.Id, out var removedNpc))
                {
                    RemoveListeners(removedNpc);
                    foreach (var player in GetAllPlayers(true))
                        Map.PacketFactory.SendNpcLeave(player.GameSession.Client, npc);
                }
            }
        }

        /// <summary>
        /// Adds listeners to npc events.
        /// </summary>
        private void AddListeners(Npc npc)
        {
            npc.MovementManager.OnMove += Npc_OnMove;

            if (npc is GuardNpc guard)
                guard.AttackManager.OnAttack += Npc_OnAttack;
        }

        /// <summary>
        /// Remove listeners from npc events.
        /// </summary>
        private void RemoveListeners(Npc npc)
        {
            npc.MovementManager.OnMove -= Npc_OnMove;

            if (npc is GuardNpc guard)
                guard.AttackManager.OnAttack -= Npc_OnAttack;
        }

        /// <summary>
        /// Gets NPC by id.
        /// </summary>
        /// <param name="includeNeighborCells">search also in neighbor cells</param>
        public Npc GetNPC(int id, bool includeNeighborCells)
        {
            Npc npc;
            NPCs.TryGetValue(id, out npc);

            if (npc is null && includeNeighborCells) // Maybe npc in neighbor cell?
                foreach (var cellId in NeighborCells)
                {
                    npc = Map.Cells[cellId].GetNPC(id, false);
                    if (npc != null)
                        break;
                }

            return npc;
        }

        /// <summary>
        /// Gets all npcs of this cell.
        /// </summary>
        /// <returns>collection of npcs</returns>
        public IEnumerable<Npc> GetAllNPCs(bool includeNeighbors)
        {
            var myNPCs = NPCs.Values;
            if (includeNeighbors)
                return myNPCs.Concat(NeighborCells.SelectMany(index => Map.Cells[index].GetAllNPCs(false))).Distinct();
            return myNPCs;
        }

        private void Npc_OnMove(int senderId, float x, float y, float z, ushort angle, MoveMotion motion)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendNpcMove(player.GameSession.Client, senderId, x, y, z, motion);
        }

        private void Npc_OnAttack(int senderId, IKillable target, AttackResult result)
        {
            foreach (var player in GetAllPlayers(true))
                Map.PacketFactory.SendNpcAttack(player.GameSession.Client, senderId, target, result);
        }

        #endregion

        #region Items

        /// <summary>
        /// Dropped items.
        /// </summary>
        private readonly ConcurrentDictionary<int, MapItem> Items = new ConcurrentDictionary<int, MapItem>();

        /// <summary>
        /// Adds item on map cell.
        /// </summary>
        /// <param name="item">new added item</param>
        public void AddItem(MapItem item)
        {
            if (Items.TryAdd(item.Id, item))
            {
                AssignCellIndex(item);
                foreach (var player in GetAllPlayers(true))
                    Map.PacketFactory.SendAddItem(player.GameSession.Client, item);
            }
        }

        /// <summary>
        /// Tries to get item from map cell.
        /// </summary>
        /// <returns>if item is null, means that item doesn't belong to player yet</returns>
        public MapItem GetItem(int itemId, Character requester, bool includeNeighborCells)
        {
            MapItem mapItem;
            if (Items.TryGetValue(itemId, out mapItem))
            {
                if (mapItem.Owner == null || mapItem.Owner == requester)
                {
                    return mapItem;
                }
                else
                {
                    return null;
                }
            }
            else // Maybe item is in neighbor cell?
            {
                if (includeNeighborCells)
                    foreach (var cellId in NeighborCells)
                    {
                        mapItem = Map.Cells[cellId].GetItem(itemId, requester, false);
                        if (mapItem != null)
                            break;
                    }

                return mapItem;
            }
        }

        /// <summary>
        /// Tries to get all items from map cell.
        /// </summary>
        /// <param name="includeNeighborCells"></param>
        public IEnumerable<MapItem> GetAllItems(bool includeNeighborCells)
        {
            List<MapItem> mapItems = new List<MapItem>();
            if (includeNeighborCells)
            {
                foreach (var cellId in NeighborCells)
                {
                    mapItems.AddRange(Map.Cells[cellId].GetAllItems(false));
                }
            }
            return Items.Values.Concat(mapItems);
        }

        /// <summary>
        /// Removes item from map.
        /// </summary>
        public MapItem RemoveItem(int itemId)
        {
            if (Items.TryRemove(itemId, out var mapItem))
            {
                mapItem.StopRemoveTimer();
                foreach (var player in GetAllPlayers(true))
                    Map.PacketFactory.SendRemoveItem(player.GameSession.Client, mapItem);
            }

            return mapItem;
        }

        #endregion

        #region Dispose

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Map));

            _isDisposed = true;

            foreach (var p in Players.Values)
                RemoveListeners(p);

            foreach (var m in Mobs.Values)
            {
                RemoveMob(m);
                m.Dispose();
            }

            foreach (var n in NPCs.Values)
                n.Dispose();

            foreach (var i in Items.Values)
                i.Dispose();

            Players.Clear();
            Mobs.Clear();
            NPCs.Clear();
            Items.Clear();

            Map = null;
        }

        #endregion
    }
}
