using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    public enum SectQuestType
    {
        Hunt,               // 狩猎任务 - 击杀指定怪物
        Gather,             // 采集任务 - 收集指定物品
        Patrol,             // 巡逻任务 - 在指定区域巡逻
        Escort,             // 护送任务 - 保护NPC到达目的地
        Investigate,        // 调查任务 - 调查异常现象
        Craft,              // 制作任务 - 制作指定物品
        Deliver,            // 递送任务 - 将物品送到指定NPC
        Subjugate,          // 镇压任务 - 镇压叛乱/邪修
    }

    public enum SectQuestDifficulty
    {
        Easy,               // 简单 - 一转蛊师可完成
        Normal,             // 普通 - 二转蛊师可完成
        Hard,               // 困难 - 三转蛊师可完成
        Expert,             // 专家 - 四转蛊师可完成
        Master,             // 大师 - 五转以上蛊师可完成
    }

    public class SectQuest
    {
        public string QuestID;
        public string DisplayName;
        public string Description;
        public SectQuestType Type;
        public SectQuestDifficulty Difficulty;
        public FactionID IssuingFaction;
        public int RequiredLevel;
        public int RewardYuanStone;
        public int RewardReputation;
        public int RewardContribution;
        public int TimeLimitDays;
        public int AcceptedDay;
        public bool IsCompleted;
        public bool IsClaimed;
        public int AcceptedByPlayerID;
    }

    public class SectQuestBoardSystem : ModSystem
    {
        public static SectQuestBoardSystem Instance => ModContent.GetInstance<SectQuestBoardSystem>();

        public Dictionary<FactionID, List<SectQuest>> AvailableQuests = new();
        public List<SectQuest> ActiveQuests = new();
        public List<string> CompletedQuestIDs = new();

        private int _lastRefreshDay = -1;
        private const int RefreshIntervalDays = 3;

        public override void OnWorldLoad()
        {
            AvailableQuests.Clear();
            ActiveQuests.Clear();
            CompletedQuestIDs.Clear();
            InitializeQuestPools();
        }

        private void InitializeQuestPools()
        {
            foreach (FactionID faction in System.Enum.GetValues<FactionID>())
            {
                if (faction == FactionID.None) continue;
                AvailableQuests[faction] = new List<SectQuest>();
            }

            RegisterQuest(new SectQuest
            {
                QuestID = "hunt_wild_beast",
                DisplayName = "猎杀野兽",
                Description = "猎杀在青茅山出没的野兽，保护村民安全。",
                Type = SectQuestType.Hunt,
                Difficulty = SectQuestDifficulty.Easy,
                RequiredLevel = 1,
                RewardYuanStone = 10,
                RewardReputation = 20,
                RewardContribution = 15,
                TimeLimitDays = 3,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "gather_herbs",
                DisplayName = "采集药材",
                Description = "为药堂采集指定药材，用于炼制丹药。",
                Type = SectQuestType.Gather,
                Difficulty = SectQuestDifficulty.Easy,
                RequiredLevel = 1,
                RewardYuanStone = 8,
                RewardReputation = 15,
                RewardContribution = 10,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "patrol_border",
                DisplayName = "边境巡逻",
                Description = "在家族领地边境巡逻，防范外敌入侵。",
                Type = SectQuestType.Patrol,
                Difficulty = SectQuestDifficulty.Normal,
                RequiredLevel = 2,
                RewardYuanStone = 20,
                RewardReputation = 30,
                RewardContribution = 25,
                TimeLimitDays = 2,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "escort_caravan",
                DisplayName = "护送商队",
                Description = "护送贾家商队安全通过危险区域。",
                Type = SectQuestType.Escort,
                Difficulty = SectQuestDifficulty.Normal,
                RequiredLevel = 2,
                RewardYuanStone = 25,
                RewardReputation = 25,
                RewardContribution = 20,
                TimeLimitDays = 3,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "investigate_anomaly",
                DisplayName = "调查异象",
                Description = "调查北荒出现的异常灵气波动。",
                Type = SectQuestType.Investigate,
                Difficulty = SectQuestDifficulty.Hard,
                RequiredLevel = 3,
                RewardYuanStone = 40,
                RewardReputation = 40,
                RewardContribution = 35,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "craft_pills",
                DisplayName = "炼制丹药",
                Description = "为家族炼制一批疗伤丹药。",
                Type = SectQuestType.Craft,
                Difficulty = SectQuestDifficulty.Hard,
                RequiredLevel = 3,
                RewardYuanStone = 35,
                RewardReputation = 30,
                RewardContribution = 30,
                TimeLimitDays = 7,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "deliver_message",
                DisplayName = "递送密信",
                Description = "将密信安全递送到白家族长处。",
                Type = SectQuestType.Deliver,
                Difficulty = SectQuestDifficulty.Normal,
                RequiredLevel = 2,
                RewardYuanStone = 15,
                RewardReputation = 20,
                RewardContribution = 15,
                TimeLimitDays = 2,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "subjugate_evil",
                DisplayName = "镇压邪修",
                Description = "镇压在禁断谷附近出没的邪修蛊师。",
                Type = SectQuestType.Subjugate,
                Difficulty = SectQuestDifficulty.Expert,
                RequiredLevel = 4,
                RewardYuanStone = 60,
                RewardReputation = 60,
                RewardContribution = 50,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "hunt_rare_beast",
                DisplayName = "猎杀稀有妖兽",
                Description = "猎杀在禁断谷深处出没的稀有妖兽。",
                Type = SectQuestType.Hunt,
                Difficulty = SectQuestDifficulty.Master,
                RequiredLevel = 5,
                RewardYuanStone = 100,
                RewardReputation = 80,
                RewardContribution = 70,
                TimeLimitDays = 7,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "gather_rare_ore",
                DisplayName = "采集稀有矿石",
                Description = "深入矿洞采集稀有矿石，用于锻造高级法器。",
                Type = SectQuestType.Gather,
                Difficulty = SectQuestDifficulty.Normal,
                RequiredLevel = 2,
                RewardYuanStone = 18,
                RewardReputation = 20,
                RewardContribution = 18,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "patrol_night",
                DisplayName = "夜间巡逻",
                Description = "在夜间巡逻家族领地，防范妖兽袭击。",
                Type = SectQuestType.Patrol,
                Difficulty = SectQuestDifficulty.Easy,
                RequiredLevel = 1,
                RewardYuanStone = 12,
                RewardReputation = 18,
                RewardContribution = 12,
                TimeLimitDays = 1,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "escort_elder",
                DisplayName = "护送长老",
                Description = "护送家族长老前往邻族参加会盟。",
                Type = SectQuestType.Escort,
                Difficulty = SectQuestDifficulty.Hard,
                RequiredLevel = 3,
                RewardYuanStone = 35,
                RewardReputation = 35,
                RewardContribution = 30,
                TimeLimitDays = 3,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "investigate_ruins",
                DisplayName = "调查遗迹",
                Description = "调查青茅山深处发现的远古遗迹。",
                Type = SectQuestType.Investigate,
                Difficulty = SectQuestDifficulty.Expert,
                RequiredLevel = 4,
                RewardYuanStone = 50,
                RewardReputation = 45,
                RewardContribution = 40,
                TimeLimitDays = 7,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "craft_weapon",
                DisplayName = "锻造法器",
                Description = "为家族锻造一批法器，需要收集材料。",
                Type = SectQuestType.Craft,
                Difficulty = SectQuestDifficulty.Normal,
                RequiredLevel = 2,
                RewardYuanStone = 22,
                RewardReputation = 25,
                RewardContribution = 22,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "deliver_supplies",
                DisplayName = "运送物资",
                Description = "将物资运送到边境哨站。",
                Type = SectQuestType.Deliver,
                Difficulty = SectQuestDifficulty.Easy,
                RequiredLevel = 1,
                RewardYuanStone = 10,
                RewardReputation = 15,
                RewardContribution = 10,
                TimeLimitDays = 3,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "subjugate_bandits",
                DisplayName = "镇压盗匪",
                Description = "镇压在商道上劫掠的盗匪团伙。",
                Type = SectQuestType.Subjugate,
                Difficulty = SectQuestDifficulty.Hard,
                RequiredLevel = 3,
                RewardYuanStone = 45,
                RewardReputation = 50,
                RewardContribution = 40,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "hunt_corrupted",
                DisplayName = "猎杀腐化生物",
                Description = "清除腐化之地蔓延的腐化生物。",
                Type = SectQuestType.Hunt,
                Difficulty = SectQuestDifficulty.Hard,
                RequiredLevel = 3,
                RewardYuanStone = 35,
                RewardReputation = 35,
                RewardContribution = 30,
                TimeLimitDays = 5,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "gather_spirit_herb",
                DisplayName = "采集灵草",
                Description = "采集稀有的灵草，用于炼制突破丹药。",
                Type = SectQuestType.Gather,
                Difficulty = SectQuestDifficulty.Expert,
                RequiredLevel = 4,
                RewardYuanStone = 45,
                RewardReputation = 40,
                RewardContribution = 35,
                TimeLimitDays = 7,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "patrol_danger_zone",
                DisplayName = "危险区域巡逻",
                Description = "在禁断谷外围巡逻，警戒异常动向。",
                Type = SectQuestType.Patrol,
                Difficulty = SectQuestDifficulty.Expert,
                RequiredLevel = 4,
                RewardYuanStone = 40,
                RewardReputation = 35,
                RewardContribution = 30,
                TimeLimitDays = 3,
            });

            RegisterQuest(new SectQuest
            {
                QuestID = "escort_tribute",
                DisplayName = "护送贡品",
                Description = "护送家族贡品前往主城。",
                Type = SectQuestType.Escort,
                Difficulty = SectQuestDifficulty.Master,
                RequiredLevel = 5,
                RewardYuanStone = 80,
                RewardReputation = 70,
                RewardContribution = 60,
                TimeLimitDays = 5,
            });
        }

        private void RegisterQuest(SectQuest quest)
        {
            foreach (var faction in AvailableQuests.Keys)
            {
                if (faction == FactionID.Scattered) continue;
                AvailableQuests[faction].Add(quest);
            }
        }

        public override void PostUpdateWorld()
        {
            int currentDay = WorldTimeHelper.CurrentDay;
            if (currentDay > _lastRefreshDay && currentDay % RefreshIntervalDays == 0)
            {
                _lastRefreshDay = currentDay;
                RefreshQuestBoards();
            }

            for (int i = ActiveQuests.Count - 1; i >= 0; i--)
            {
                var quest = ActiveQuests[i];
                int elapsedDays = currentDay - quest.AcceptedDay;
                if (elapsedDays > quest.TimeLimitDays && !quest.IsCompleted)
                {
                    if (quest.AcceptedByPlayerID >= 0 && quest.AcceptedByPlayerID < Main.maxPlayers)
                    {
                        var player = Main.player[quest.AcceptedByPlayerID];
                        if (player.active && player.whoAmI == Main.myPlayer)
                            Main.NewText($"宗门任务「{quest.DisplayName}」已过期！", Microsoft.Xna.Framework.Color.Gray);
                    }
                    ActiveQuests.RemoveAt(i);
                }
            }
        }

        private void RefreshQuestBoards()
        {
            foreach (var kvp in AvailableQuests)
            {
                var faction = kvp.Key;
                var quests = kvp.Value;

                var shuffled = new List<SectQuest>(quests);
                int n = shuffled.Count;
                while (n > 1)
                {
                    n--;
                    int k = Main.rand.Next(n + 1);
                    var temp = shuffled[k];
                    shuffled[k] = shuffled[n];
                    shuffled[n] = temp;
                }

                AvailableQuests[faction] = shuffled.GetRange(0, System.Math.Min(5, shuffled.Count));
            }
        }

        public List<SectQuest> GetAvailableQuests(FactionID faction)
        {
            if (AvailableQuests.TryGetValue(faction, out var quests))
                return quests;
            return new List<SectQuest>();
        }

        public bool AcceptQuest(Player player, string questID)
        {
            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            var faction = guWorld.CurrentAlly;

            if (!AvailableQuests.TryGetValue(faction, out var quests)) return false;

            var quest = quests.Find(q => q.QuestID == questID);
            if (quest == null) return false;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < quest.RequiredLevel)
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText("修为不足，无法接受此任务。", Microsoft.Xna.Framework.Color.Red);
                return false;
            }

            quest.AcceptedDay = WorldTimeHelper.CurrentDay;
            quest.AcceptedByPlayerID = player.whoAmI;
            ActiveQuests.Add(quest);
            AvailableQuests[faction].Remove(quest);

            if (player.whoAmI == Main.myPlayer)
                Main.NewText($"接受了宗门任务：{quest.DisplayName}", Microsoft.Xna.Framework.Color.Yellow);

            return true;
        }

        public void CompleteQuest(Player player, string questID)
        {
            var quest = ActiveQuests.Find(q => q.QuestID == questID && q.AcceptedByPlayerID == player.whoAmI);
            if (quest == null || quest.IsCompleted) return;

            quest.IsCompleted = true;

            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            guWorld.AddReputation(quest.IssuingFaction, quest.RewardReputation, "完成任务");

            var rankPlayer = player.GetModPlayer<FactionRankPlayer>();
            rankPlayer.AddContribution(quest.IssuingFaction, quest.RewardContribution);

            player.QuickSpawnItem(player.GetSource_GiftOrReward(),
                ModContent.ItemType<Content.Items.Consumables.YuanS>(), quest.RewardYuanStone);

            CompletedQuestIDs.Add(questID);

            if (player.whoAmI == Main.myPlayer)
            {
                Main.NewText($"宗门任务完成：{quest.DisplayName}！获得{quest.RewardYuanStone}元石、{quest.RewardReputation}声望、{quest.RewardContribution}贡献。",
                    Microsoft.Xna.Framework.Color.Green);
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var activeList = new List<TagCompound>();
            foreach (var quest in ActiveQuests)
            {
                activeList.Add(new TagCompound
                {
                    ["questID"] = quest.QuestID,
                    ["faction"] = (int)quest.IssuingFaction,
                    ["acceptedDay"] = quest.AcceptedDay,
                    ["playerID"] = quest.AcceptedByPlayerID,
                    ["isCompleted"] = quest.IsCompleted,
                });
            }
            tag["sectActiveQuests"] = activeList;
            tag["sectCompletedIDs"] = new List<string>(CompletedQuestIDs);
            tag["sectLastRefresh"] = _lastRefreshDay;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            ActiveQuests.Clear();
            CompletedQuestIDs.Clear();

            var activeList = tag.GetList<TagCompound>("sectActiveQuests");
            if (activeList != null)
            {
                foreach (var t in activeList)
                {
                    var quest = new SectQuest
                    {
                        QuestID = t.GetString("questID"),
                        IssuingFaction = (FactionID)t.GetInt("faction"),
                        AcceptedDay = t.GetInt("acceptedDay"),
                        AcceptedByPlayerID = t.GetInt("playerID"),
                        IsCompleted = t.GetBool("isCompleted"),
                    };
                    ActiveQuests.Add(quest);
                }
            }

            var completedList = tag.GetList<string>("sectCompletedIDs");
            if (completedList != null)
                CompletedQuestIDs.AddRange(completedList);

            _lastRefreshDay = tag.GetInt("sectLastRefresh");
        }
    }
}