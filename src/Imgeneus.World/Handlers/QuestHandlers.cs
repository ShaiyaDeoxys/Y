using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Game;
using Imgeneus.World.Game.Quests;
using Imgeneus.World.Game.Session;
using Imgeneus.World.Game.Zone;
using Imgeneus.World.Packets;
using Sylver.HandlerInvoker.Attributes;
using System.Threading.Tasks;

namespace Imgeneus.World.Handlers
{
    [Handler]
    public class QuestHandlers : BaseHandler
    {
        private readonly IQuestsManager _questsManager;

        public QuestHandlers(IGamePacketFactory packetFactory, IGameSession gameSession, IQuestsManager questsManager) : base(packetFactory, gameSession)
        {
            _questsManager = questsManager;
        }

        [HandlerAction(PacketType.QUEST_START)]
        public async Task HandleQuestStart(WorldClient client, QuestStartPacket packet)
        {
            var ok = await _questsManager.TryStartQuest(packet.NpcId, packet.QuestId);
            if (ok)
                _packetFactory.SendQuestStarted(client, packet.NpcId, packet.QuestId);
        }

        [HandlerAction(PacketType.QUEST_END)]
        public void HandleQuestEnd(WorldClient client, QuestEndPacket packet)
        {
            var ok = _questsManager.TryFinishQuest(packet.NpcId, packet.QuestId, out var quest);
            if (!ok)
                _packetFactory.SendQuestFinished(client, packet.NpcId, packet.QuestId, quest, ok);
            else
                if (quest.CanChooseRevard)
                _packetFactory.SendQuestChooseRevard(client, packet.QuestId);

        }

        [HandlerAction(PacketType.QUEST_END_SELECT)]
        public void HandleQuestEndSelect(WorldClient client, QuestEndSelectPacket packet)
        {
            _questsManager.TryFinishQuestSelect(packet.NpcId, packet.QuestId, packet.Index);
        }

        [HandlerAction(PacketType.QUEST_QUIT)]
        public void HandleQuitQuest(WorldClient client, QuestQuitPacket packet)
        {
            _questsManager.QuitQuest(packet.QuestId);
        }
    }
}
