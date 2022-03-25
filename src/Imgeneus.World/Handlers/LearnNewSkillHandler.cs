using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Skills;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class LearnNewSkillHandler : BaseHandler
    {
        private readonly ISkillsManager _skillsManager;

        public LearnNewSkillHandler(IGamePacketFactory packetFactory, IGameSession gameSession, ISkillsManager skillsManager) : base(packetFactory, gameSession)
        {
            _skillsManager = skillsManager;
        }

        [HandlerAction(PacketType.LEARN_NEW_SKILL)]
        public void Handle(WorldClient client, LearnNewSkillPacket packet)
        {
            var result = _skillsManager.TryLearnNewSkill(packet.SkillId, packet.SkillLevel);
            _packetFactory.SendLearnedNewSkill(client, result.Ok, result.Skill);
        }
    }
}
