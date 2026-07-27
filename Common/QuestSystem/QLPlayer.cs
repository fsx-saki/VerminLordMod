using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Common.QuestSystem.QLNodes;

namespace VerminLordMod.Common.QuestSystem
{
    public class QLPlayer : ModPlayer
    {
        public Dictionary<string, QuestSaveData> QuestProgress = [];

        public override void SaveData(TagCompound tag)
        {
            try
            {
                TagCompound questsTag = [];
                foreach (var kvp in QuestProgress)
                    questsTag[kvp.Key] = kvp.Value.Serialize();
                tag["QuestProgress"] = questsTag;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<VerminLordMod>().Logger.Error(
                    $"[QLPlayer:SaveData] {ex.Message}");
            }
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                QuestProgress = [];
                if (tag.TryGet("QuestProgress", out TagCompound questsTag))
                {
                    foreach (var kvp in questsTag)
                    {
                        if (kvp.Value is TagCompound questDataTag)
                            QuestProgress[kvp.Key] = QuestSaveData.Deserialize(questDataTag);
                    }
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<VerminLordMod>().Logger.Error(
                    $"[QLPlayer:LoadData] {ex.Message}");
            }
        }

        public QuestSaveData GetQuestData(string questID)
        {
            if (!QuestProgress.ContainsKey(questID))
                QuestProgress[questID] = QuestSaveData.Default;
            return QuestProgress[questID];
        }

        public override void OnEnterWorld()
        {
            var firstQuest = QuestNode.GetQuest<FirstQuest>();
            if (firstQuest != null)
                firstQuest.IsUnlocked = true;

            foreach (var quest in QuestNode.AllQuests)
            {
                quest.OnWorldEnter();
                quest.CheckUnlock();
            }
        }

        public override void PostUpdate()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            bool checkUnlock = Main.GameUpdateCount % 60 == 0;

            foreach (var quest in QuestNode.AllQuests)
            {
                if (checkUnlock && !quest.IsUnlocked)
                    quest.CheckUnlock();

                if (quest.IsUnlocked && !quest.IsCompleted)
                    quest.UpdateByPlayer();
            }
        }
    }
}
