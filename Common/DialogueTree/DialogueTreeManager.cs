// ============================================================
// DialogueTreeManager - 对话树核心管理器
// 负责注册、查询、执行对话树，管理对话会话
// ============================================================
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话树管理器 — 对话树系统的核心
/// </summary>
public class DialogueTreeManager
{
    // ===== 单例 =====
    public static DialogueTreeManager Instance { get; } = new();

    // ===== 对话树注册表 =====
    private readonly Dictionary<string, DialogueTree> _treesByID = new();
    private readonly Dictionary<int, string> _treeIDByNPCType = new();

    // ===== 当前对话会话 =====
    public DialogueSession CurrentSession { get; private set; } = new();

    // ============================================================
    // 注册
    // ============================================================

    /// <summary> 注册一个对话树 </summary>
    public void RegisterTree(DialogueTree tree)
    {
        _treesByID[tree.TreeID] = tree;
        if (tree.NPCType.HasValue)
        {
            _treeIDByNPCType[tree.NPCType.Value] = tree.TreeID;
        }
    }

    /// <summary> 注册一个对话树并关联NPC类型 </summary>
    public void RegisterTree<T>(DialogueTree tree) where T : ModNPC
    {
        tree.NPCType = ModContent.NPCType<T>();
        RegisterTree(tree);
    }

    // ============================================================
    // 查询
    // ============================================================

    /// <summary> 获取NPC对应的对话树 </summary>
    public DialogueTree GetTree(NPC npc)
    {
        if (_treeIDByNPCType.TryGetValue(npc.type, out var treeID))
        {
            return GetTree(treeID);
        }
        return null;
    }

    /// <summary> 通过ID获取对话树 </summary>
    public DialogueTree GetTree(string treeID)
    {
        return _treesByID.TryGetValue(treeID, out var tree) ? tree : null;
    }

    /// <summary> 获取对话树中的节点 </summary>
    public DialogueNode GetNode(string treeID, string nodeID)
    {
        var tree = GetTree(treeID);
        return tree?.GetNode(nodeID);
    }

    /// <summary> 检查NPC是否有对话树 </summary>
    public bool HasTree(NPC npc)
    {
        return _treeIDByNPCType.ContainsKey(npc.type);
    }

    /// <summary> 检查NPC是否有对话树（通过类型） </summary>
    public bool HasTree<T>() where T : ModNPC
    {
        return _treeIDByNPCType.ContainsKey(ModContent.NPCType<T>());
    }

    // ============================================================
    // 执行
    // ============================================================

    /// <summary> 检查玩家是否有活跃的对话会话 </summary>
    public bool HasActiveSession(Player player)
    {
        return CurrentSession.IsActive && CurrentSession.CurrentPlayer == player;
    }

    /// <summary> 获取玩家当前对话的节点ID </summary>
    public string GetCurrentNodeID(Player player)
    {
        if (!HasActiveSession(player)) return null;
        return CurrentSession.CurrentNodeID;
    }

    /// <summary> 开始与NPC的对话 </summary>
    public bool StartDialogue(NPC npc, Player player)
    {
        var tree = GetTree(npc);
        if (tree == null) return false;

        var rootNode = tree.GetRootNode();
        if (rootNode == null) return false;

        CurrentSession.Start(npc, player, tree.TreeID, tree.RootNodeID);

        // 执行根节点的进入效果
        ExecuteEffects(rootNode.OnEnterEffects, player, npc);

        return true;
    }

    /// <summary> 选择当前节点的某个选项（带玩家验证） </summary>
    public bool SelectOption(Player player, int optionIndex)
    {
        if (!HasActiveSession(player)) return false;
        return SelectOption(optionIndex);
    }

    /// <summary> 选择当前节点的某个选项 </summary>
    public bool SelectOption(int optionIndex)
    {
        if (!CurrentSession.IsActive) return false;

        var tree = GetTree(CurrentSession.CurrentNPC);
        if (tree == null) return false;

        var node = tree.GetNode(CurrentSession.CurrentNodeID);
        if (node == null) return false;

        var belief = GetBelief(CurrentSession.CurrentNPC, CurrentSession.CurrentPlayer);
        var visibleOptions = node.GetVisibleOptions(CurrentSession.CurrentPlayer, CurrentSession.CurrentNPC, belief);

        if (optionIndex < 0 || optionIndex >= visibleOptions.Count) return false;

        var option = visibleOptions[optionIndex];

        // 执行节点离开效果
        ExecuteEffects(node.OnExitEffects, CurrentSession.CurrentPlayer, CurrentSession.CurrentNPC);

        // 执行选项效果
        ExecuteEffects(option.Effects, CurrentSession.CurrentPlayer, CurrentSession.CurrentNPC);

        // 处理特殊操作
        if (option.OpensShop != null)
        {
            // 打开商店 — 由外部处理
            return true;
        }

        if (option.TargetNodeID == null)
        {
            // 结束对话
            CurrentSession.End();
            return true;
        }

        // 进入目标节点
        CurrentSession.EnterNode(option.TargetNodeID);
        var targetNode = tree.GetNode(option.TargetNodeID);
        if (targetNode != null)
        {
            ExecuteEffects(targetNode.OnEnterEffects, CurrentSession.CurrentPlayer, CurrentSession.CurrentNPC);

            if (targetNode.EndsDialogue)
            {
                CurrentSession.End();
            }
        }

        return true;
    }

    /// <summary> 返回上一个节点 </summary>
    public bool GoBack()
    {
        if (!CurrentSession.IsActive) return false;
        return CurrentSession.GoBack();
    }

    /// <summary> 结束当前对话（带玩家验证） </summary>
    public void EndDialogue(Player player)
    {
        if (!HasActiveSession(player)) return;
        EndDialogue();
    }

    /// <summary> 结束当前对话 </summary>
    public void EndDialogue()
    {
        CurrentSession.End();
    }

    /// <summary> 获取当前节点的可见选项（带玩家验证） </summary>
    public List<DialogueOption> GetCurrentOptions(Player player)
    {
        if (!HasActiveSession(player)) return new List<DialogueOption>();
        return GetCurrentOptions();
    }

    /// <summary> 获取当前节点的可见选项 </summary>
    public List<DialogueOption> GetCurrentOptions()
    {
        if (!CurrentSession.IsActive) return new List<DialogueOption>();

        var tree = GetTree(CurrentSession.CurrentNPC);
        if (tree == null) return new List<DialogueOption>();

        var node = tree.GetNode(CurrentSession.CurrentNodeID);
        if (node == null) return new List<DialogueOption>();

        var belief = GetBelief(CurrentSession.CurrentNPC, CurrentSession.CurrentPlayer);
        return node.GetVisibleOptions(CurrentSession.CurrentPlayer, CurrentSession.CurrentNPC, belief);
    }

    /// <summary> 获取当前节点的NPC文本（带玩家验证） </summary>
    public string GetCurrentNPCText(Player player)
    {
        if (!HasActiveSession(player)) return "";
        return GetCurrentNPCText();
    }

    /// <summary> 获取当前节点的NPC文本 </summary>
    public string GetCurrentNPCText()
    {
        if (!CurrentSession.IsActive) return "";

        var tree = GetTree(CurrentSession.CurrentNPC);
        if (tree == null) return "";

        var node = tree.GetNode(CurrentSession.CurrentNodeID);
        if (node == null) return "";

        return FormatNPCText(node.NPCText, CurrentSession.CurrentNPC);
    }

    // ============================================================
    // 辅助方法
    // ============================================================

    private void ExecuteEffects(List<DialogueEffect> effects, Player player, NPC npc)
    {
        var belief = GetBelief(npc, player);
        foreach (var effect in effects)
        {
            effect.Execute(player, npc, belief);
        }
    }

    private static BeliefState GetBelief(NPC npc, Player player)
    {
        if (npc.ModNPC is IGuMasterAI guAI)
        {
            return guAI.GetBelief(player.name);
        }
        return null;
    }

    /// <summary>
    /// 格式化NPC文本（替换占位符）
    /// </summary>
    private static string FormatNPCText(string text, NPC npc)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string npcName = npc.GivenName ?? npc.TypeName;
        return text.Replace("{npcName}", npcName);
    }

    /// <summary>
    /// 清除所有注册的对话树（用于Unload）
    /// </summary>
    public void Unload()
    {
        _treesByID.Clear();
        _treeIDByNPCType.Clear();
        CurrentSession.End();
    }
}
