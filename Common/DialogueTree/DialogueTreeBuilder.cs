// ============================================================
// DialogueTreeBuilder - 对话树构建器（Fluent API）
// 提供链式调用来构建对话树
// ============================================================
using System.Collections.Generic;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话树构建器 — 使用 Fluent API 构建对话树
/// </summary>
public class DialogueTreeBuilder
{
    private readonly DialogueTree _tree = new();
    private DialogueNode _currentNode;

    public DialogueTreeBuilder(string treeID, string rootNodeID)
    {
        _tree.TreeID = treeID;
        _tree.RootNodeID = rootNodeID;
    }

    /// <summary> 开始创建一个新节点 </summary>
    public DialogueTreeBuilder StartNode(string nodeID, string npcText)
    {
        _currentNode = new DialogueNode(nodeID, npcText);
        _tree.Nodes[nodeID] = _currentNode;
        return this;
    }

    /// <summary> 添加一个选项 </summary>
    public DialogueTreeBuilder AddOption(string text, string targetNodeID,
        DialogueOptionType optionType = DialogueOptionType.Normal,
        DialogueCondition condition = null,
        string tooltip = null,
        string opensShop = null)
    {
        var option = new DialogueOption(text, targetNodeID, optionType)
        {
            Condition = condition,
            Tooltip = tooltip,
            OpensShop = opensShop
        };
        _currentNode.Options.Add(option);
        return this;
    }

    /// <summary> 添加一个带效果的选项 </summary>
    public DialogueTreeBuilder AddOptionWithEffect(string text, string targetNodeID,
        DialogueEffect effect,
        DialogueOptionType optionType = DialogueOptionType.Normal,
        DialogueCondition condition = null,
        string tooltip = null)
    {
        var option = new DialogueOption(text, targetNodeID, optionType)
        {
            Condition = condition,
            Tooltip = tooltip
        };
        option.Effects.Add(effect);
        _currentNode.Options.Add(option);
        return this;
    }

    /// <summary> 添加一个带多个效果的选项（params 参数） </summary>
    public DialogueTreeBuilder AddOptionWithEffects(string text, string targetNodeID,
        params DialogueEffect[] effects)
    {
        return AddOptionWithEffects(text, targetNodeID, DialogueOptionType.Normal, null, null, effects);
    }

    /// <summary> 添加一个带多个效果的选项（完整参数） </summary>
    public DialogueTreeBuilder AddOptionWithEffects(string text, string targetNodeID,
        DialogueOptionType optionType,
        DialogueCondition condition,
        string tooltip,
        params DialogueEffect[] effects)
    {
        var option = new DialogueOption(text, targetNodeID, optionType)
        {
            Condition = condition,
            Tooltip = tooltip,
        };
        option.Effects.AddRange(effects);
        _currentNode.Options.Add(option);
        return this;
    }

    /// <summary> 设置节点进入效果 </summary>
    public DialogueTreeBuilder WithEnterEffect(DialogueEffect effect)
    {
        _currentNode.OnEnterEffects.Add(effect);
        return this;
    }

    /// <summary> 设置节点离开效果 </summary>
    public DialogueTreeBuilder WithExitEffect(DialogueEffect effect)
    {
        _currentNode.OnExitEffects.Add(effect);
        return this;
    }

    /// <summary> 设置节点结束对话 </summary>
    public DialogueTreeBuilder EndsDialogue()
    {
        _currentNode.EndsDialogue = true;
        return this;
    }

    /// <summary> 设置节点触发战斗 </summary>
    public DialogueTreeBuilder TriggersCombat()
    {
        _currentNode.TriggersCombat = true;
        return this;
    }

    /// <summary> 设置节点打开商店 </summary>
    public DialogueTreeBuilder OpensShop(string shopName)
    {
        _currentNode.OpensShop = shopName;
        return this;
    }

    /// <summary> 设置节点标签 </summary>
    public DialogueTreeBuilder WithLabel(string label)
    {
        _currentNode.Label = label;
        return this;
    }

    /// <summary> 构建对话树 </summary>
    public DialogueTree Build()
    {
        return _tree;
    }
}
