// ============================================================
// DialogueSession - 对话会话状态
// 管理一次对话的完整生命周期
// ============================================================
using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话会话 — 管理一次对话的完整状态
/// </summary>
public class DialogueSession
{
    /// <summary> 当前对话的NPC </summary>
    public NPC CurrentNPC;

    /// <summary> 当前对话的玩家 </summary>
    public Player CurrentPlayer;

    /// <summary> 当前对话树ID </summary>
    public string CurrentTreeID;

    /// <summary> 当前节点ID </summary>
    public string CurrentNodeID;

    /// <summary> 上一个节点ID </summary>
    public string PreviousNodeID;

    /// <summary> 对话历史栈（用于返回功能） </summary>
    public Stack<string> NodeHistory = new();

    /// <summary> 本次对话的临时标记 </summary>
    public Dictionary<string, bool> LocalFlags = new();

    /// <summary> 对话是否活跃 </summary>
    public bool IsActive => CurrentNPC != null && CurrentNPC.active;

    /// <summary>
    /// 进入一个新节点
    /// </summary>
    public void EnterNode(string nodeID)
    {
        if (!string.IsNullOrEmpty(CurrentNodeID))
        {
            NodeHistory.Push(CurrentNodeID);
        }
        PreviousNodeID = CurrentNodeID;
        CurrentNodeID = nodeID;
    }

    /// <summary>
    /// 返回上一个节点
    /// </summary>
    public bool GoBack()
    {
        if (NodeHistory.Count > 0)
        {
            PreviousNodeID = CurrentNodeID;
            CurrentNodeID = NodeHistory.Pop();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 设置一个本地标记
    /// </summary>
    public void SetFlag(string flagName, bool value = true)
    {
        LocalFlags[flagName] = value;
    }

    /// <summary>
    /// 获取一个本地标记
    /// </summary>
    public bool GetFlag(string flagName)
    {
        return LocalFlags.TryGetValue(flagName, out var value) && value;
    }

    /// <summary>
    /// 清除所有本地标记
    /// </summary>
    public void ClearFlags()
    {
        LocalFlags.Clear();
    }

    /// <summary>
    /// 开始新的对话会话
    /// </summary>
    public void Start(NPC npc, Player player, string treeID, string rootNodeID)
    {
        CurrentNPC = npc;
        CurrentPlayer = player;
        CurrentTreeID = treeID;
        CurrentNodeID = rootNodeID;
        PreviousNodeID = null;
        NodeHistory.Clear();
        LocalFlags.Clear();
    }

    /// <summary>
    /// 结束对话会话
    /// </summary>
    public void End()
    {
        CurrentNPC = null;
        CurrentPlayer = null;
        CurrentTreeID = null;
        CurrentNodeID = null;
        PreviousNodeID = null;
        NodeHistory.Clear();
        LocalFlags.Clear();
    }
}
