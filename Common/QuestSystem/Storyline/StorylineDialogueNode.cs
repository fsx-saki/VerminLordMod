using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.QuestSystem.Storyline
{
    /// <summary>
    /// 主线对话节点 — 比 DialogueNode 更丰富的结构。
    /// 支持内嵌物品发放标记 [item:ClassName:描述]。
    /// </summary>
    public class StorylineDialogueNode
    {
        /// <summary>节点 ID（唯一标识）</summary>
        public string Id;

        /// <summary>系统（"莲"）的台词行列表</summary>
        public List<string> SystemLines = [];

        /// <summary>玩家选项文本</summary>
        public string ChoiceText = "";

        /// <summary>下一节点 ID（空 = 对话结束）</summary>
        public string NextNodeId = "";

        /// <summary>此节点触发时自动发放的物品（类型名→数量）</summary>
        public Dictionary<string, int> AutoGiveItems = [];

        /// <summary>是否已发放过自动物品</summary>
        public bool AutoItemsGiven;

        public StorylineDialogueNode() { }

        public StorylineDialogueNode(string id, List<string> lines, string choice, string next)
        {
            Id = id;
            SystemLines = lines;
            ChoiceText = choice;
            NextNodeId = next;
        }

        public StorylineDialogueNode(string id, List<string> lines, string choice, string next,
            Dictionary<string, int> autoGive)
            : this(id, lines, choice, next)
        {
            AutoGiveItems = autoGive;
        }
    }

    /// <summary>
    /// 玩家级别的主线任务运行时数据。
    /// 仿照 QuestSaveData 的设计——扁平、可序列化。
    /// </summary>
    public class StorylineQuestData
    {
        // ══════════════════════════════════════════════════════
        // 对话状态
        // ══════════════════════════════════════════════════════
        public string CurrentDialogueNodeId = "start";
        public bool DialogueFinished;
        public List<string> UnlockedNodeIds = [];

        // ══════════════════════════════════════════════════════
        // 奖励状态
        // ══════════════════════════════════════════════════════
        public bool RewardsClaimed;

        // ══════════════════════════════════════════════════════
        // 序列化
        // ══════════════════════════════════════════════════════

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["CurNode"] = CurrentDialogueNodeId ?? "",
                ["DlgDone"] = DialogueFinished,
                ["Unlocked"] = UnlockedNodeIds ?? [],
                ["Rewards"] = RewardsClaimed
            };
        }

        public static StorylineQuestData Deserialize(TagCompound tag)
        {
            var data = new StorylineQuestData();
            if (tag.TryGet("CurNode", out string cur)) data.CurrentDialogueNodeId = cur;
            if (tag.TryGet("DlgDone", out bool done)) data.DialogueFinished = done;
            if (tag.TryGet("Unlocked", out List<string> ul)) data.UnlockedNodeIds = ul ?? [];
            if (tag.TryGet("Rewards", out bool r)) data.RewardsClaimed = r;
            return data;
        }

        public static StorylineQuestData Default => new()
        {
            CurrentDialogueNodeId = "start",
            DialogueFinished = false,
            UnlockedNodeIds = [],
            RewardsClaimed = false
        };
    }
}
