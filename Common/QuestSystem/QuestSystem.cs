using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Common.QuestSystem.QLNodes;

namespace VerminLordMod.Common.QuestSystem
{
    public class QuestSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            QuestNotificationSystem.Update();
        }

        public override void OnWorldLoad()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            var firstQuest = QuestNode.GetQuest<FirstQuest>();
            if (firstQuest != null && !firstQuest.IsCompleted)
            {
                firstQuest.IsUnlocked = true;
            }

            foreach (var quest in QuestNode.AllQuests)
            {
                quest.OnWorldEnter();
                quest.CheckUnlock();
            }
        }
    }
}
