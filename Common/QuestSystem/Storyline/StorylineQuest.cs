using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Common.QuestSystem.Dialogue;

namespace VerminLordMod.Common.QuestSystem.Storyline
{
    /// <summary>
    /// 主线任务基类 — 融合对话树 + 条件触发 + 奖励发放。
    ///
    /// 【PRT 风格的设计哲学】
    /// 仿照 BasePRT 的"模板+改参数"模式：
    ///   1. 继承 StorylineQuest
    ///   2. 在 SetStaticDefaults() 中设置对话树、条件、奖励
    ///   3. 其余生命周期（解锁检查/对话推进/完成收纳）全自动
    ///
    /// 【快速上手模板】
    /// <code>
    /// public class MyStoryQuest : StorylineQuest
    /// {
    ///     public override void SetStaticDefaults()
    ///     {
    ///         QuestType = QuestType.Main;
    ///         Difficulty = QuestDifficulty.Normal;
    ///
    ///         // 1. 设置触发条件
    ///         TriggerConditions.Add(StorylineConditions.Always());
    ///
    ///         // 2. 设置对话树
    ///         DialogueNodes = new List<StorylineDialogueNode>
    ///         {
    ///             new("start", ["你好，实验者。"], "你好", "next"),
    ///             new("next", ["任务已接收。"], "明白", ""),
    ///         };
    ///
    ///         // 3. 设置奖励
    ///         AddReward(ItemID.GoldCoin, 5);
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class StorylineQuest : QuestNode
    {
        // ══════════════════════════════════════════════════════
        // 对话树（按 PRT 风格：在 SetStaticDefaults 中填充）
        // ══════════════════════════════════════════════════════

        /// <summary>该任务的对话节点列表</summary>
        public List<StorylineDialogueNode> DialogueNodes = [];

        /// <summary>起始节点 ID（默认 "start"）</summary>
        public string StartNodeId = "start";

        // ══════════════════════════════════════════════════════
        // 触发条件（AND 逻辑：全部满足才解锁）
        // ══════════════════════════════════════════════════════

        /// <summary>触发条件列表 — 全部满足后自动解锁</summary>
        public List<IStorylineCondition> TriggerConditions = [];

        // ══════════════════════════════════════════════════════
        // 运行时状态（通过 StorylinePlayer 管理）
        // ══════════════════════════════════════════════════════

        /// <summary>获取连接到当前玩家的 Storyline 数据</summary>
        protected StorylineQuestData Data
        {
            get
            {
                var sp = Main.LocalPlayer?.GetModPlayer<StorylinePlayer>();
                return sp?.GetData(ID);
            }
        }

        /// <summary>当前对话节点 ID</summary>
        public string CurrentDialogueNodeId
        {
            get => Data?.CurrentDialogueNodeId ?? StartNodeId;
            set { if (Data != null) Data.CurrentDialogueNodeId = value; }
        }

        /// <summary>对话是否已完成</summary>
        public bool DialogueFinished
        {
            get => Data?.DialogueFinished ?? false;
            set { if (Data != null) Data.DialogueFinished = value; }
        }

        /// <summary>奖励是否已领取</summary>
        public bool RewardsClaimed
        {
            get => Data?.RewardsClaimed ?? false;
            set { if (Data != null) Data.RewardsClaimed = value; }
        }

        /// <summary>是否已被收纳进资料库（对话完成+奖励领取后）</summary>
        public bool IsArchived => DialogueFinished && RewardsClaimed;

        /// <summary>获取当前对话节点</summary>
        public StorylineDialogueNode GetCurrentDialogueNode()
            => DialogueNodes.Find(n => n.Id == CurrentDialogueNodeId);

        /// <summary>通过 ID 获取对话节点</summary>
        public StorylineDialogueNode GetDialogueNode(string id)
            => DialogueNodes.Find(n => n.Id == id);

        // ══════════════════════════════════════════════════════
        // 生命周期（PRT 风格：构造→初始化→每帧更新→绘制）
        // ══════════════════════════════════════════════════════

        public override void VaultSetup()
        {
            base.VaultSetup();

            // 构建 DialogueNode 查找字典
            _nodeLookup = DialogueNodes.ToDictionary(n => n.Id, n => n);
        }

        private Dictionary<string, StorylineDialogueNode> _nodeLookup;

        /// <summary>条件检查 — 由 StorylinePlayer 每 60 tick 调用</summary>
        public bool CheckTriggerConditions()
        {
            if (TriggerConditions.Count == 0) return true;

            var player = Main.LocalPlayer;
            if (player == null) return false;

            return TriggerConditions.TrueForAll(c => c.IsMet(player));
        }

        public override void UpdateByPlayer()
        {
            // Storyline 任务不在此接口更新统一检查触发条件。
            // 触发条件检测由 StorylinePlayer.PostUpdate() 管理。
        }

        /// <summary>
        /// 对话推进。返回当前节点（推进后）。
        /// 如果对话完成，自动标记 DialogueFinished。
        /// </summary>
        public StorylineDialogueNode AdvanceDialogue()
        {
            var current = GetCurrentDialogueNode();
            if (current == null) return null;

            // 记录已解锁节点
            if (Data != null && !Data.UnlockedNodeIds.Contains(current.Id))
                Data.UnlockedNodeIds.Add(current.Id);

            if (string.IsNullOrEmpty(current.NextNodeId))
            {
                DialogueFinished = true;
                return null;
            }

            CurrentDialogueNodeId = current.NextNodeId;
            return GetCurrentDialogueNode();
        }

        /// <summary>
        /// 领取奖励。将物品发放给玩家，标记任务为已完成并收入资料库。
        /// </summary>
        public bool ClaimRewards()
        {
            if (!DialogueFinished || RewardsClaimed) return false;

            var player = Main.LocalPlayer;
            if (player == null) return false;

            foreach (var reward in Rewards)
            {
                if (reward.Claimed) continue;
                player.QuickSpawnItem(player.GetSource_GiftOrReward(), reward.ItemType, reward.Amount);
                reward.Claimed = true;
            }

            RewardsClaimed = true;
            IsCompleted = true;
            return true;
        }
    }
}
