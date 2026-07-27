using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Common.QuestSystem.QLNodes;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.QuestSystem.Storyline
{
    /// <summary>
    /// 主线任务玩家数据 — 管理所有 StorylineQuest 的运行时状态。
    ///
    /// 【PRT 风格生命周期】
    ///   OnEnterWorld → 初始化/恢复数据
    ///   PostUpdate  → 每 60 tick 检查触发条件
    ///   SaveData     → 序列化每个任务的状态
    /// </summary>
    public class StorylinePlayer : ModPlayer
    {
        /// <summary>所有主线任务的运行时数据（questId → data）</summary>
        public Dictionary<string, StorylineQuestData> QuestData = [];

        /// <summary>标记已在首次进入世界时完成自动处理的任务</summary>
        private readonly HashSet<string> _autoProcessedQuests = [];

        /// <summary>获取指定任务的数据（自动创建默认值）</summary>
        public StorylineQuestData GetData(string questId)
        {
            if (!QuestData.ContainsKey(questId))
                QuestData[questId] = StorylineQuestData.Default;
            return QuestData[questId];
        }

        // ══════════════════════════════════════════════════════
        // 序列化
        // ══════════════════════════════════════════════════════

        public override void SaveData(TagCompound tag)
        {
            try
            {
                TagCompound questsTag = [];
                foreach (var kvp in QuestData)
                    questsTag[kvp.Key] = kvp.Value.Serialize();
                tag["SL_Quests"] = questsTag;
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<VerminLordMod>().Logger.Error(
                    $"[StorylinePlayer:SaveData] {ex.Message}");
            }
        }

        public override void LoadData(TagCompound tag)
        {
            try
            {
                QuestData = [];
                if (tag.TryGet("SL_Quests", out TagCompound questsTag))
                {
                    foreach (var kvp in questsTag)
                    {
                        if (kvp.Value is TagCompound t)
                            QuestData[kvp.Key] = StorylineQuestData.Deserialize(t);
                    }
                }
            }
            catch (Exception ex)
            {
                ModContent.GetInstance<VerminLordMod>().Logger.Error(
                    $"[StorylinePlayer:LoadData] {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════
        // 世界进入 — 自动处理 LotusIntroQuest
        // ══════════════════════════════════════════════════════

        public override void OnEnterWorld()
        {
            // 确保所有 StorylineQuest 数据条目存在
            foreach (var quest in QuestNode.AllQuests.OfType<StorylineQuest>())
            {
                if (!QuestData.ContainsKey(quest.ID))
                    QuestData[quest.ID] = StorylineQuestData.Default;
            }

            // 自动处理 LotusIntroQuest：推进所有对话，发放奖励
            AutoProcessQuest<QLNodes.LotusIntroQuest>();
        }

        /// <summary>
        /// 自动处理指定主线任务：推进全部对话节点完成 → 标记对话结束 → 发放所有奖励。
        /// </summary>
        private void AutoProcessQuest<T>() where T : StorylineQuest
        {
            var rawQuest = QuestNode.GetQuest<T>();
            if (rawQuest is not T quest) return;
            if (_autoProcessedQuests.Contains(quest.ID)) return;

            var data = GetData(quest.ID);
            if (data == null) return;

            // 如果已经归档（对话完成+奖励领取），跳过
            if (quest.IsArchived) return;

            // 确保任务已解锁
            if (!quest.IsUnlocked) return;

            // 推进所有对话节点
            quest.CurrentDialogueNodeId = quest.StartNodeId;
            while (!quest.DialogueFinished)
            {
                var node = quest.GetCurrentDialogueNode();
                if (node == null) break;

                // 记录已解锁节点
                if (!data.UnlockedNodeIds.Contains(node.Id))
                    data.UnlockedNodeIds.Add(node.Id);

                if (string.IsNullOrEmpty(node.NextNodeId))
                {
                    quest.DialogueFinished = true;
                    break;
                }

                data.CurrentDialogueNodeId = node.NextNodeId;
            }

            // 发放奖励
            if (quest.DialogueFinished && !data.RewardsClaimed)
            {
                var player = Main.LocalPlayer;
                if (player != null)
                {
                    foreach (var reward in quest.Rewards)
                    {
                        if (reward.Claimed) continue;
                        player.QuickSpawnItem(player.GetSource_GiftOrReward(), reward.ItemType, reward.Amount);
                        reward.Claimed = true;
                    }
                    data.RewardsClaimed = true;
                    quest.IsCompleted = true;

                    Main.NewText($"[莲] 主线引导已完成，奖励已发放。", 180, 210, 255);
                }
            }

            _autoProcessedQuests.Add(quest.ID);
        }

        // ══════════════════════════════════════════════════════
        // 每帧更新 — 检查触发条件 + 检测元海激活
        // ══════════════════════════════════════════════════════

        private bool _yuanHaiWasActivated; // 缓存上一帧的元海激活状态
        private bool _characterPanelUnlocked; // 是否已解锁人物面板

        public override void PostUpdate()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server) return;

            // 每 60 tick 检查一次（约 1 秒），避免每帧计算
            if (Main.GameUpdateCount % 60 != 0) return;

            var yhFlags = Main.LocalPlayer?.GetModPlayer<YuanHaiFlags>();

            // ════════════════════════════════════════════════
            // 检测元海激活 → 解锁人物面板 + 触发第二主线
            // ════════════════════════════════════════════════
            if (yhFlags != null && yhFlags.Activated && !_yuanHaiWasActivated)
            {
                OnYuanHaiActivated(yhFlags);
            }
            _yuanHaiWasActivated = yhFlags?.Activated ?? false;

            // ════════════════════════════════════════════════
            // 检查所有 StorylineQuest 的触发条件
            // ════════════════════════════════════════════════
            foreach (var quest in QuestNode.AllQuests.OfType<StorylineQuest>())
            {
                if (quest.IsUnlocked) continue;
                if (quest.IsArchived) continue;

                if (!quest.CheckTriggerConditions()) continue;

                bool preReqsMet = quest.ParentIDs.Count == 0
                    || quest.ParentIDs.TrueForAll(pid =>
                    {
                        var parent = QuestNode.GetQuest(pid);
                        return parent != null && parent.IsCompleted;
                    });

                if (preReqsMet)
                {
                    quest.IsUnlocked = true;
                    ModContent.GetInstance<VerminLordMod>().Logger.Info(
                        $"[StorylinePlayer] Unlocked: {quest.ID}");
                }
            }
        }

        /// <summary>
        /// 元海激活时的回调。
        /// 1. 解锁"人物面板"（显示一条系统通知）
        /// 2. 自动解锁第二个主线任务（YuanHaiActivationQuest）
        /// </summary>
        private void OnYuanHaiActivated(YuanHaiFlags flags)
        {
            // 解锁人物面板通知
            if (!_characterPanelUnlocked)
            {
                _characterPanelUnlocked = true;
                Main.NewText("[莲] 元海已激活。人物面板已解锁。按 Y 打开元海界面。", 100, 180, 220);
            }

            // 自动解锁第二个主线任务
            var secondQuest = QuestNode.GetQuest<QLNodes.YuanHaiActivationQuest>();
            if (secondQuest != null && !secondQuest.IsUnlocked)
            {
                secondQuest.IsUnlocked = true;
                Main.NewText($"[莲] 新主线已解锁：{secondQuest.DisplayName?.Value ?? secondQuest.ID}", 220, 200, 120);
            }
        }

        // ══════════════════════════════════════════════════════
        // 辅助查询
        // ══════════════════════════════════════════════════════

        /// <summary>获取当前活跃的主线任务（主页上显示的）</summary>
        public StorylineQuest GetActiveStorylineQuest()
        {
            // 返回第一个已解锁但未归档的主线任务
            return QuestNode.AllQuests
                .OfType<StorylineQuest>()
                .FirstOrDefault(q => q.IsUnlocked && !q.IsArchived);
        }

        /// <summary>获取所有已归档的主线任务（资料库用）</summary>
        public List<StorylineQuest> GetArchivedQuests()
        {
            return QuestNode.AllQuests
                .OfType<StorylineQuest>()
                .Where(q => q.IsArchived)
                .ToList();
        }

        /// <summary>获取所有进行中的主线任务</summary>
        public List<StorylineQuest> GetInProgressQuests()
        {
            return QuestNode.AllQuests
                .OfType<StorylineQuest>()
                .Where(q => q.IsUnlocked && !q.IsArchived)
                .ToList();
        }
    }
}
