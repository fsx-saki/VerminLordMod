using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // QuestSystem — 任务/委托系统大框
    //
    // 职责：
    // 1. 管理所有可用的任务定义（QuestDefinition）
    // 2. 订阅 QuestCompletedEvent，处理任务完成逻辑
    // 3. 根据玩家声望/修为/剧情阶段解锁新任务
    // 4. 提供任务查询接口（NPC 可查询可领取的任务列表）
    //
    // TODO:
    //   - 填充具体任务定义数据（QuestRegistry）
    //   - 实现任务条件检查逻辑
    //   - 实现任务奖励发放逻辑
    //   - 实现任务UI显示
    //   - 实现任务失败/超时逻辑
    // ============================================================

    public enum QuestType
    {
        MainStory,      // 主线剧情任务
        FactionQuest,   // 家族委托
        SideQuest,      // 支线任务
        DailyQuest,     // 每日委托
        BountyQuest,    // 悬赏任务
        CollectionQuest // 收集任务
    }

    public enum QuestState
    {
        Locked,         // 未解锁
        Available,      // 可领取
        Active,         // 进行中
        Completed,      // 已完成
        Failed,         // 已失败
        Expired         // 已过期
    }

    public enum QuestRewardType
    {
        Reputation,     // 声望
        YuanStones,     // 元石
        Item,           // 物品
        QiExp,          // 真元经验
        StoryProgress   // 剧情推进
    }

    public class QuestDefinition
    {
        public string QuestID;
        public string Title;
        public string Description;
        public string BriefDescription;
        public QuestType Type;
        public FactionID IssuingFaction;
        public int RequiredGuLevel;
        public int RequiredRepLevel;
        public StoryPhase RequiredStoryPhase;
        public List<string> PrerequisiteQuestIDs = new();
        public List<QuestObjective> Objectives = new();
        public List<QuestReward> Rewards = new();
        public int ExpireDays;
        public bool IsRepeatable;
        public string IssuingNPCType;
    }

    public class QuestObjective
    {
        public string ObjectiveID;
        public string Description;
        public ObjectiveType Type;
        public int TargetAmount;
        public int CurrentAmount;
        public bool IsCompleted => CurrentAmount >= TargetAmount;
        public string TargetNPCName;
        public int TargetItemType;
        public int TargetNPCType;
        public FactionID TargetFaction;
    }

    public enum ObjectiveType
    {
        KillNPC,            // 杀指定NPC
        KillNPCType,        // 杀指定类型NPC
        CollectItem,        // 收集物品
        ReachRepLevel,      // 达到声望等级
        EnterTerritory,     // 进入领地
        TalkToNPC,          // 与NPC对话
        DefendWave,         // 防守波次
        CompleteStoryPhase, // 完成剧情阶段
        CraftItem,          // 炼化物品
        SearchNode,         // 探索资源节点
        SurviveTribulation  // 渡过天劫
    }

    public class QuestReward
    {
        public QuestRewardType Type;
        public int Amount;
        public FactionID TargetFaction;
        public int ItemType;
        public string StoryFlag;
    }

    public class QuestInstance
    {
        public string QuestID;
        public QuestState State;
        public int AcceptedDay;
        public int ExpireDay;
        public List<QuestObjective> Objectives = new();
        public bool IsExpired => ExpireDay > 0 && (int)(Main.time / 36000) > ExpireDay;
        public bool IsCompleted => Objectives.TrueForAll(o => o.IsCompleted);
    }

    public class QuestSystem : ModSystem
    {
        public static QuestSystem Instance => ModContent.GetInstance<QuestSystem>();

        public Dictionary<string, QuestDefinition> QuestRegistry = new();

        public override void OnWorldLoad()
        {
            QuestRegistry.Clear();
            RegisterAllQuests();
        }

        public override void OnWorldUnload()
        {
            QuestRegistry.Clear();
        }

        private bool _isSubscribed = false;

        public override void PostUpdateWorld()
        {
            if (!_isSubscribed)
            {
                EventBus.Subscribe<QuestCompletedEvent>(OnQuestCompleted);
                EventBus.Subscribe<NPCDeathEvent>(OnNPCDeathForQuest);
                _isSubscribed = true;
            }
        }

        private void RegisterAllQuests()
        {
            // TODO: 注册所有任务定义
            // RegisterGuYueMainStoryQuests();
            // RegisterGuYueFactionQuests();
            // RegisterDailyQuests();
            // RegisterBountyQuests();
        }

        public List<QuestDefinition> GetAvailableQuests(Player player)
        {
            var tracker = player.GetModPlayer<QuestTrackerPlayer>();
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var guWorld = player.GetModPlayer<GuWorldPlayer>();
            var storyPhase = StoryManager.Instance.GetPhase(player);

            var available = new List<QuestDefinition>();
            foreach (var quest in QuestRegistry.Values)
            {
                if (tracker.HasActiveQuest(quest.QuestID)) continue;
                if (tracker.HasCompletedQuest(quest.QuestID) && !quest.IsRepeatable) continue;

                // TODO: 实现完整的条件检查
                // if (qiRealm.GuLevel < quest.RequiredGuLevel) continue;
                // if (guWorld.GetRepLevel(quest.IssuingFaction).ordinal < quest.RequiredRepLevel) continue;
                // if (storyPhase < quest.RequiredStoryPhase) continue;
                // if (!AllPrerequisitesCompleted(tracker, quest)) continue;

                available.Add(quest);
            }
            return available;
        }

        public QuestDefinition GetQuestDefinition(string questID)
        {
            QuestRegistry.TryGetValue(questID, out var def);
            return def;
        }

        public QuestInstance CreateQuestInstance(QuestDefinition definition)
        {
            var instance = new QuestInstance
            {
                QuestID = definition.QuestID,
                State = QuestState.Active,
                AcceptedDay = (int)(Main.time / 36000),
                ExpireDay = definition.ExpireDays > 0
                    ? (int)(Main.time / 36000) + definition.ExpireDays
                    : 0,
                Objectives = new List<QuestObjective>()
            };

            foreach (var objDef in definition.Objectives)
            {
                instance.Objectives.Add(new QuestObjective
                {
                    ObjectiveID = objDef.ObjectiveID,
                    Description = objDef.Description,
                    Type = objDef.Type,
                    TargetAmount = objDef.TargetAmount,
                    CurrentAmount = 0,
                    TargetNPCName = objDef.TargetNPCName,
                    TargetItemType = objDef.TargetItemType,
                    TargetNPCType = objDef.TargetNPCType,
                    TargetFaction = objDef.TargetFaction
                });
            }

            return instance;
        }

        public void CompleteQuest(Player player, string questID)
        {
            var tracker = player.GetModPlayer<QuestTrackerPlayer>();
            var instance = tracker.GetActiveQuest(questID);
            if (instance == null) return;

            instance.State = QuestState.Completed;
            GrantRewards(player, QuestRegistry[questID]);
            tracker.RemoveActiveQuest(questID);
            tracker.AddCompletedQuest(questID);

            EventBus.Publish(new QuestCompletedEvent
            {
                PlayerID = player.whoAmI,
                QuestID = questID,
                QuestType = QuestRegistry[questID].Type,
                IssuingFaction = QuestRegistry[questID].IssuingFaction
            });
        }

        private void GrantRewards(Player player, QuestDefinition quest)
        {
            // TODO: 实现奖励发放
            foreach (var reward in quest.Rewards)
            {
                switch (reward.Type)
                {
                    case QuestRewardType.Reputation:
                        // player.GetModPlayer<GuWorldPlayer>().AddReputation(reward.TargetFaction, reward.Amount);
                        break;
                    case QuestRewardType.YuanStones:
                        // player.QuickSpawnItem(Item.GetSource_GiftOrReward(), ModContent.ItemType<YuanS>(), reward.Amount);
                        break;
                    case QuestRewardType.Item:
                        // player.QuickSpawnItem(Item.GetSource_GiftOrReward(), reward.ItemType, reward.Amount);
                        break;
                    case QuestRewardType.QiExp:
                        // TODO: 真元经验奖励
                        break;
                    case QuestRewardType.StoryProgress:
                        // StoryManager.Instance.CompleteStory(player, reward.StoryFlag);
                        break;
                }
            }
        }

        public void OnQuestCompleted(QuestCompletedEvent evt)
        {
            // TODO: 任务完成后续处理（解锁新任务、通知家族等）
        }

        private void OnNPCDeathForQuest(NPCDeathEvent evt)
        {
            // TODO: 检查是否有击杀NPC类型相关的任务目标
            // 更新所有拥有 KillNPCType 目标的活跃任务进度
        }
    }

    // ============================================================
    // QuestTrackerPlayer — 玩家任务追踪器
    //
    // 职责：
    // 1. 存储玩家的活跃任务列表和已完成任务记录
    // 2. 提供任务领取/放弃/完成接口
    // 3. 每帧更新任务目标进度
    // 4. Save/Load 任务进度
    //
    // TODO:
    //   - 实现目标进度自动更新（击杀/收集/对话等）
    //   - 实现任务超时检查
    //   - 实现任务UI交互
    // ============================================================

    public class QuestTrackerPlayer : ModPlayer
    {
        public List<QuestInstance> ActiveQuests = new();
        public HashSet<string> CompletedQuestIDs = new();
        public Dictionary<string, int> CompletedQuestCounts = new();

        public bool HasActiveQuest(string questID)
        {
            return ActiveQuests.Exists(q => q.QuestID == questID);
        }

        public bool HasCompletedQuest(string questID)
        {
            return CompletedQuestIDs.Contains(questID);
        }

        public QuestInstance GetActiveQuest(string questID)
        {
            return ActiveQuests.Find(q => q.QuestID == questID);
        }

        public void AcceptQuest(string questID)
        {
            var system = QuestSystem.Instance;
            var definition = system.GetQuestDefinition(questID);
            if (definition == null) return;

            var instance = system.CreateQuestInstance(definition);
            ActiveQuests.Add(instance);

            Main.NewText($"接受任务：{definition.Title}", Microsoft.Xna.Framework.Color.Yellow);
        }

        public void AbandonQuest(string questID)
        {
            var instance = GetActiveQuest(questID);
            if (instance == null) return;

            ActiveQuests.Remove(instance);

            var definition = QuestSystem.Instance.GetQuestDefinition(questID);
            Main.NewText($"放弃任务：{definition?.Title ?? questID}", Microsoft.Xna.Framework.Color.Gray);
        }

        public void RemoveActiveQuest(string questID)
        {
            ActiveQuests.RemoveAll(q => q.QuestID == questID);
        }

        public void AddCompletedQuest(string questID)
        {
            CompletedQuestIDs.Add(questID);
            if (!CompletedQuestCounts.ContainsKey(questID))
                CompletedQuestCounts[questID] = 0;
            CompletedQuestCounts[questID]++;
        }

        public void UpdateObjectiveProgress(ObjectiveType type, int amount, int targetType = -1)
        {
            // TODO: 实现目标进度自动更新
            foreach (var quest in ActiveQuests)
            {
                foreach (var obj in quest.Objectives)
                {
                    if (obj.Type == type && (targetType == -1 || obj.TargetItemType == targetType || obj.TargetNPCType == targetType))
                    {
                        obj.CurrentAmount += amount;
                    }
                }

                if (quest.IsCompleted)
                {
                    QuestSystem.Instance.CompleteQuest(Player, quest.QuestID);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            // TODO: 实现任务进度保存
            var activeQuestIDs = new List<string>();
            foreach (var quest in ActiveQuests)
            {
                activeQuestIDs.Add(quest.QuestID);
            }
            tag["ActiveQuestIDs"] = activeQuestIDs;
            tag["CompletedQuestIDs"] = CompletedQuestIDs;
        }

        public override void LoadData(TagCompound tag)
        {
            // TODO: 实现任务进度加载
            ActiveQuests.Clear();
            CompletedQuestIDs.Clear();

            var activeQuestIDs = tag.GetList<string>("ActiveQuestIDs");
            var completedList = tag.GetList<string>("CompletedQuestIDs");
            CompletedQuestIDs = new HashSet<string>(completedList);
        }
    }

    // ============================================================
    // 任务相关事件
    // ============================================================

    public class QuestCompletedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
        public QuestType QuestType;
        public FactionID IssuingFaction;
    }

    public class QuestAcceptedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
    }

    public class QuestAbandonedEvent : GuWorldEvent
    {
        public int PlayerID;
        public string QuestID;
    }
}