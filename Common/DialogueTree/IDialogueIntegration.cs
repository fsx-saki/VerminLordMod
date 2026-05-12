using Terraria;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 对话集成接口 — 连接对话系统与游戏子系统。
    ///
    /// 设计意图：
    /// - 解耦 DialogueEffect/DialogueCondition 与具体 ModPlayer/System 的直接依赖
    /// - 所有游戏状态查询和修改都通过此接口，而非直接访问 GuMasterBase/GuWorldPlayer 等
    /// - 便于单元测试（可 mock）和跨系统复用
    ///
    /// 层级位置：Layer 4 — Integration Layer（集成层）
    /// 上层依赖：DialogueEffect, DialogueCondition（Layer 0 — Data Layer）
    /// 下层依赖：GuWorldPlayer, QiRealmPlayer, GuMasterBase 等游戏系统
    /// </summary>
    public interface IDialogueIntegration
    {
        // ===== 信念系统 =====

        BeliefState GetBelief(Player player, NPC npc);
        void SetBeliefField(Player player, NPC npc, BeliefField field, float value);
        void ModifyBeliefField(Player player, NPC npc, BeliefField field, float delta);

        // ===== 声望系统 =====

        int GetReputation(Player player, FactionID faction);
        void AddReputation(Player player, FactionID faction, int amount, string reason);
        void RemoveReputation(Player player, FactionID faction, int amount, string reason);

        // ===== 修为系统 =====

        int GetGuLevel(Player player);

        // ===== 物品系统 =====

        bool HasItem(Player player, int itemType, int minStack);
        void GiveItem(Player player, int itemType, int stack);
        bool RemoveItem(Player player, int itemType, int stack);

        // ===== 态度系统 =====

        GuAttitude GetAttitude(Player player, NPC npc);
        void SetAttitude(Player player, NPC npc, GuAttitude attitude);

        // ===== 标记系统 =====

        bool GetFlag(Player player, string flagName);
        void SetFlag(Player player, string flagName, bool value);

        // ===== 元石系统 =====

        int GetYuanS(Player player);
        bool SpendYuanS(Player player, int amount);
        void GiveYuanS(Player player, int amount);
    }

    /// <summary>
    /// 信念字段枚举 — 标识 BeliefState 中的可修改字段
    /// </summary>
    public enum BeliefField
    {
        RiskThreshold,
        ConfidenceLevel,
        EstimatedPower
    }
}