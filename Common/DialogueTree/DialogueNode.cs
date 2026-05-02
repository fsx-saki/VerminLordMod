// ============================================================
// DialogueNode - 对话树节点数据结构
// 对话树的基本单元，包含NPC文本、选项列表、进入/离开效果
// ============================================================
using System.Collections.Generic;
using Terraria;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话选项类型（用于UI样式区分）
/// </summary>
public enum DialogueOptionType
{
    Normal,      // 普通选项（白色）
    Informative, // 情报选项（蓝色）
    Risky,       // 冒险选项（红色）
    Trade,       // 交易选项（金色）
    Special,     // 特殊选项（紫色）
    Exit,        // 退出选项（灰色）
    Combat,      // 战斗选项（深红）
    Barter,      // 议价选项（橙色）
    Quest,       // 任务选项（青色）
    Social,      // 社交选项（粉色）
    Craft,       // 炼制选项（绿色）
    Teach,       // 教学选项（亮蓝）
    Steal,       // 偷窃选项（暗紫）
    Deceive,     // 欺骗选项（暗红）
    Ally,        // 结盟选项（亮金）
    Betray,      // 背叛选项（暗金）
}

/// <summary>
/// 对话选项 — 玩家可以选择的一个回答
/// </summary>
public class DialogueOption
{
    /// <summary> 选项显示文本 </summary>
    public string Text;

    /// <summary> 目标节点ID（null表示结束对话或打开商店） </summary>
    public string TargetNodeID;

    /// <summary> 显示此选项的条件（为空则始终显示） </summary>
    public DialogueCondition Condition;

    /// <summary> 选择此选项时触发的效果列表 </summary>
    public List<DialogueEffect> Effects = new();

    /// <summary> 选项类型（用于UI样式区分） </summary>
    public DialogueOptionType OptionType = DialogueOptionType.Normal;

    /// <summary> 工具提示文本 </summary>
    public string Tooltip;

    /// <summary> 是否打开商店（设置商店名称） </summary>
    public string OpensShop;

    public DialogueOption() { }

    public DialogueOption(string text, string targetNodeID, DialogueOptionType optionType = DialogueOptionType.Normal)
    {
        Text = text;
        TargetNodeID = targetNodeID;
        OptionType = optionType;
    }
}

/// <summary>
/// 对话节点 — 对话树的基本单元
/// </summary>
public class DialogueNode
{
    /// <summary> 节点唯一标识 </summary>
    public string NodeID;

    /// <summary> NPC说的文本（支持{placeholder}格式化） </summary>
    public string NPCText;

    /// <summary> 对话选项列表 </summary>
    public List<DialogueOption> Options = new();

    /// <summary> 进入此节点时触发的效果 </summary>
    public List<DialogueEffect> OnEnterEffects = new();

    /// <summary> 离开此节点时触发的效果 </summary>
    public List<DialogueEffect> OnExitEffects = new();

    /// <summary> 是否终止对话 </summary>
    public bool EndsDialogue;

    /// <summary> 是否触发战斗 </summary>
    public bool TriggersCombat;

    /// <summary> 是否打开商店 </summary>
    public string OpensShop;

    /// <summary> 节点标签（用于调试） </summary>
    public string Label;

    public DialogueNode() { }

    public DialogueNode(string nodeID, string npcText)
    {
        NodeID = nodeID;
        NPCText = npcText;
    }

    /// <summary>
    /// 获取当前节点中可见的选项列表（经过条件过滤）
    /// </summary>
    public List<DialogueOption> GetVisibleOptions(Player player, NPC npc, BeliefState belief)
    {
        var visible = new List<DialogueOption>();
        foreach (var option in Options)
        {
            if (option.Condition == null || option.Condition.Evaluate(player, npc, belief))
            {
                visible.Add(option);
            }
        }
        return visible;
    }
}

/// <summary>
/// 对话树 — 完整的对话树定义
/// </summary>
public class DialogueTree
{
    /// <summary> 对话树唯一标识 </summary>
    public string TreeID;

    /// <summary> 根节点ID </summary>
    public string RootNodeID;

    /// <summary> 所有节点的字典 </summary>
    public Dictionary<string, DialogueNode> Nodes = new();

    /// <summary> 关联的NPC类型（可选） </summary>
    public int? NPCType;

    public DialogueNode GetNode(string nodeID)
    {
        return Nodes.TryGetValue(nodeID, out var node) ? node : null;
    }

    public DialogueNode GetRootNode()
    {
        return GetNode(RootNodeID);
    }
}
