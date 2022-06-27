using Imgeneus.Database.Entities;
using Imgeneus.World.Game.Country;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.Zone;
using Microsoft.Extensions.Logging;
using Parsec.Shaiya.Common;
using Parsec.Shaiya.NpcQuest;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Imgeneus.World.Game.NPCs
{
    public class Npc : IMapMember, IDisposable
    {
        private readonly ILogger _logger;
        private readonly BaseNpc _npc;
        protected readonly List<(float X, float Y, float Z, ushort Angle)> _moveCoordinates;

        public IMovementManager MovementManager { get; private set; }
        public ICountryProvider CountryProvider { get; private set; }
        public IMapProvider MapProvider { get; private set; }

        public Npc(ILogger<Npc> logger, BaseNpc npc, List<(float X, float Y, float Z, ushort Angle)> moveCoordinates, IMovementManager movementManager, ICountryProvider countryProvider, IMapProvider mapProvider) : this(npc)
        {
            _logger = logger;
            _npc = npc;
            _moveCoordinates = moveCoordinates;

            MovementManager = movementManager;
            CountryProvider = countryProvider;
            MapProvider = mapProvider;
        }

        public Npc(BaseNpc npc)
        {
            // Set products.
            if (npc is Merchant merchant)
            {
                foreach (var item in merchant.SaleItems)
                {
                    try
                    {
                        _products.Add(new NpcProduct(item.Type, item.TypeId));
                    }
                    catch
                    {
                        _logger.LogError("Couldn't parse npc item definition, plase check this npc: ({type}, {typeId}).", npc.Type, npc.TypeId);
                    }
                }
            }

            // Set quests.
            foreach (var quest in npc.InQuestIds)
                _startQuestIds.Add(quest);

            foreach (var quest in npc.OutQuestIds)
                _endQuestIds.Add(quest);

            // Set teleport gates.
            if (npc is GateKeeper gatekeeper)
            {
                foreach(var gate in gatekeeper.GateTargets)
                    _gates.Add(gate);
            }
        }

        public virtual void Init(uint ownerId)
        {
            Id = ownerId;

            MovementManager.Init(Id, _moveCoordinates[0].X, _moveCoordinates[0].Y, _moveCoordinates[0].Z, _moveCoordinates[0].Angle, MoveMotion.Run);

            Fraction country;
            switch (_npc.Faction)
            {
                case NpcFaction.Light:
                    country = Fraction.Light;
                    break;

                case NpcFaction.Fury:
                    country = Fraction.Dark;
                    break;

                default:
                    country = Fraction.NotSelected;
                    break;
            }
            CountryProvider.Init(Id, country);
        }

        public uint Id { get; set; }

        #region Position

        public float PosX { get => MovementManager.PosX; }

        public float PosY { get => MovementManager.PosY; }

        public float PosZ { get => MovementManager.PosZ; }

        public ushort Angle { get => MovementManager.Angle; }

        #endregion

        public Map Map { get => MapProvider.Map; set => MapProvider.Map = value; }

        public int CellId { get => MapProvider.CellId; set => MapProvider.CellId = value; }

        public int OldCellId { get => MapProvider.OldCellId; set => MapProvider.OldCellId = value; }

        /// <summary>
        /// Type of NPC.
        /// </summary>
        public NpcType Type { get => _npc.Type; }

        /// <summary>
        /// Type id of NPC.
        /// </summary>
        public short TypeId { get => _npc.TypeId; }

        #region Products

        private readonly IList<NpcProduct> _products = new List<NpcProduct>();
        private IList<NpcProduct> _readonlyProducts;

        /// <summary>
        /// Items, that npc sells.
        /// </summary>
        public IList<NpcProduct> Products
        {
            get
            {
                if (_readonlyProducts is null)
                    _readonlyProducts = new ReadOnlyCollection<NpcProduct>(_products);
                return _readonlyProducts;
            }
        }

        /// <summary>
        /// Checks if Product list contains product at index. Logs warning, if product is not found.
        /// </summary>
        /// <param name="index">index, that we want to check.</param>
        /// <returns>return true, if there is some product at index</returns>
        public bool ContainsProduct(byte index)
        {
            if (Products.Count <= index)
            {
                _logger.LogWarning("NPC ({type}, {typeId}) doesn't contain product at index {index}. Check it out.", _npc.Type, _npc.TypeId, index);
                return false;
            }

            return true;
        }

        #endregion

        #region Start quests

        private readonly IList<short> _startQuestIds = new List<short>();
        private IList<short> _readonlyStartQuestIds;

        /// <summary>
        /// Collection of quests, that player can start at this npc.
        /// </summary>
        public IList<short> StartQuestIds
        {
            get
            {
                if (_readonlyStartQuestIds is null)
                    _readonlyStartQuestIds = new ReadOnlyCollection<short>(_startQuestIds);

                return _readonlyStartQuestIds;
            }
        }

        #endregion

        #region End quests

        private readonly IList<short> _endQuestIds = new List<short>();
        private IList<short> _readonlyEndQuestIds;

        /// <summary>
        /// Collection of quests, that player can start at this npc.
        /// </summary>
        public IList<short> EndQuestIds
        {
            get
            {
                if (_readonlyEndQuestIds is null)
                    _readonlyEndQuestIds = new ReadOnlyCollection<short>(_endQuestIds);

                return _readonlyEndQuestIds;
            }
        }

        #endregion

        #region Gates

        private readonly IList<GateTarget> _gates = new List<GateTarget>();
        private IList<GateTarget> _readonlyGates;

        /// <summary>
        /// Gates, where npc can teleport.
        /// </summary>
        public IList<GateTarget> Gates
        {
            get
            {
                if (_readonlyGates is null)
                    _readonlyGates = new ReadOnlyCollection<GateTarget>(_gates);
                return _readonlyGates;
            }
        }

        /// <summary>
        /// Checks if Gate list contains gate at index. Logs warning, if gate is not found.
        /// </summary>
        /// <param name="index">index, that we want to check.</param>
        /// <returns>return true, if there is some gate at index</returns>
        public bool ContainsGate(byte index)
        {
            if (Gates.Count <= index)
            {
                _logger.LogWarning("NPC {type}, {typeId} doesn't contain gate at index {index}. Check it out.", _npc.Type, _npc.TypeId, index);
                return false;
            }

            return true;
        }

        #endregion

        #region Dispose

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Npc));

            _isDisposed = true;

            _products.Clear();
            _startQuestIds.Clear();
            _endQuestIds.Clear();

            Map = null;
        }

        #endregion
    }
}
