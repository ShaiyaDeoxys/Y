using Imgeneus.Core.Extensions;
using Imgeneus.World.Game.Buffs;
using Imgeneus.World.Game.Movement;
using Imgeneus.World.Game.PartyAndRaid;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Timers;

namespace Imgeneus.Game.Skills
{
    public class CastProtectionManager : ICastProtectionManager
    {
        private readonly ILogger<CastProtectionManager> _logger;
        private readonly IMovementManager _movementManager;
        private readonly IPartyManager _partyProvider;
        private readonly IGamePacketFactory _packetFactory;
        private readonly IGameSession _gameSession;
        private readonly IMapProvider _mapProvider;
        private uint _ownerId;

        public CastProtectionManager(ILogger<CastProtectionManager> logger, IMovementManager movementManager, IPartyManager partyProvider, IGamePacketFactory packetFactory, IGameSession gameSession, IMapProvider mapProvider)
        {
            _logger = logger;
            _movementManager = movementManager;
            _partyProvider = partyProvider;
            _packetFactory = packetFactory;
            _gameSession = gameSession;
            _mapProvider = mapProvider;
            _checkProtectCastingSkill.Elapsed += CheckProtectCastingSkill_Elapsed;
            _checkProtectCastingSkill.Start();

#if DEBUG
            _logger.LogDebug("CastProtectionManager {hashcode} created", GetHashCode());
#endif
        }

#if DEBUG
        ~CastProtectionManager()
        {
            _logger.LogDebug("CastProtectionManager {hashcode} collected by GC", GetHashCode());
        }
#endif

        #region Init & Clear

        public void Init(uint ownerId)
        {
            _ownerId = ownerId;
        }

        public void Dispose()
        {
            _checkProtectCastingSkill.Stop();
            _checkProtectCastingSkill.Elapsed -= CheckProtectCastingSkill_Elapsed;
        }

        #endregion

        public bool IsCastProtected
        {
            get
            {
                if (!_partyProvider.HasParty)
                    return false;

                return _partyProvider.Party.Members.Any(m => m.Id != _ownerId &&
                                            m.CastProtectionManager.ProtectAlliesCasting &&
                                            m.MapProvider.Map.Id == _mapProvider.Map.Id &&
                                            MathExtensions.Distance(m.MovementManager.PosX, _movementManager.PosX, m.MovementManager.PosZ, _movementManager.PosZ) <= m.CastProtectionManager.ProtectCastingRange);
            }
        }

        public bool ProtectAlliesCasting { get; set; }

        public byte ProtectCastingRange { get; set; }

        public (ushort SkillId, byte SkillLevel) ProtectCastingSkill { get; set; }

        #region Fake protect skill

        /// <summary>
        /// This timer will check each second if player is protected by any other player.
        /// If so, fake buff will be added.
        /// </summary>
        private Timer _checkProtectCastingSkill = new Timer() { Interval = 1000, AutoReset = true };

        private (ushort SkillId, byte SkillLevel) _lastProtectCastingSkill = (0, 0);
        private void CheckProtectCastingSkill_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_partyProvider.HasParty)
            {
                if (_lastProtectCastingSkill != (0, 0))
                {
                    _packetFactory.SendRemoveBuff(_gameSession.Client, uint.MaxValue);
                    _lastProtectCastingSkill = (0, 0);
                }

                return;
            }

            var protector = _partyProvider.Party.Members
                            .FirstOrDefault(m => m.Id != _ownerId &&
                                            m.CastProtectionManager.ProtectAlliesCasting &&
                                            m.MapProvider.Map.Id == _mapProvider.Map.Id &&
                                            MathExtensions.Distance(m.MovementManager.PosX, _movementManager.PosX, m.MovementManager.PosZ, _movementManager.PosZ) <= m.CastProtectionManager.ProtectCastingRange);

            if (protector != null && _lastProtectCastingSkill != protector.CastProtectionManager.ProtectCastingSkill)
            {
                _packetFactory.SendAddBuff(_gameSession.Client, uint.MaxValue, protector.CastProtectionManager.ProtectCastingSkill.SkillId, protector.CastProtectionManager.ProtectCastingSkill.SkillLevel, 0);
                _lastProtectCastingSkill = protector.CastProtectionManager.ProtectCastingSkill;
                return;
            }

            if (protector is null)
            {
                if (_lastProtectCastingSkill != (0, 0))
                {
                    _packetFactory.SendRemoveBuff(_gameSession.Client, uint.MaxValue);
                    _lastProtectCastingSkill = (0, 0);
                }

                return;
            }
        }

        #endregion

        public bool ReduceCastingTime { get; set; }
    }
}
