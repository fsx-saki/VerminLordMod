using System.Collections.Generic;
using Terraria;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 对话内容提供者接口 — 任何能参与对话的实体都应实现此接口。
    ///
    /// 设计意图：
    /// - 解耦 DialogueSystem 与 GuMasterBase 的具体类型依赖
    /// - 允许 NPC、物品、世界对象等不同实体提供对话
    /// - 对话树由 Provider 自行构建，Manager 只负责会话生命周期
    ///
    /// 层级位置：Layer 1 — Content Layer（内容层）
    /// 上层依赖：DialogueTreeManager（Layer 2 — Session Layer）
    /// 下层依赖：DialogueNode, DialogueTree（Layer 0 — Data Layer）
    /// </summary>
    public interface IDialogueProvider
    {
        /// <summary>
        /// 获取此实体的显示名称（用于对话 UI 标题栏）
        /// </summary>
        string GetDisplayName();

        /// <summary>
        /// 获取 NPC 头像类型（用于对话 UI），-1 表示无头像
        /// </summary>
        int GetHeadType();

        /// <summary>
        /// 是否可以与此实体对话
        /// </summary>
        bool CanTalk(Player player);

        /// <summary>
        /// 获取问候文本（对话开始时 NPC 说的第一句话）
        /// 在对话树加载前显示，给玩家一个"进入对话"的过渡
        /// </summary>
        string GetGreetingText(Player player);

        /// <summary>
        /// 获取此实体的对话树。
        /// 返回 null 表示使用默认对话系统（非对话树模式）。
        /// </summary>
        DialogueTree GetDialogueTree(Player player);

        /// <summary>
        /// 获取对话选项（非对话树模式时使用）。
        /// 返回 null 表示使用默认选项。
        /// </summary>
        List<DialogueOption> GetDefaultOptions(Player player);

        /// <summary>
        /// 获取此实体关联的信念状态（如果有）。
        /// 返回 null 表示此实体不参与信念系统。
        /// </summary>
        BeliefState GetBelief(Player player);

        /// <summary>
        /// 获取此实体关联的 NPC 实例（如果有）。
        /// 返回 null 表示此实体不是 NPC（如物品对话）。
        /// </summary>
        NPC GetNPC();
    }
}