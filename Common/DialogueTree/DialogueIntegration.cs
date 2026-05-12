using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 默认对话集成实现 — 桥接对话系统与现有游戏子系统。
    ///
    /// 所有游戏状态查询和修改都通过此实现，而非直接访问具体类型。
    /// 当 NPC 不是 GuMasterBase 时，信念/态度相关操作会优雅降级。
    /// </summary>
    public class DialogueIntegration : IDialogueIntegration
    {
        public static DialogueIntegration Instance { get; } = new();

        private DialogueIntegration() { }

        // ===== 信念系统 =====

        public BeliefState GetBelief(Player player, NPC npc)
        {
            if (npc.ModNPC is GuMasterBase guMaster)
                return guMaster.GetBelief(player.name);
            return null;
        }

        public void SetBeliefField(Player player, NPC npc, BeliefField field, float value)
        {
            var belief = GetBelief(player, npc);
            if (belief == null) return;

            value = MathHelper.Clamp(value, 0f, 1f);

            switch (field)
            {
                case BeliefField.RiskThreshold:
                    belief.RiskThreshold = value;
                    break;
                case BeliefField.ConfidenceLevel:
                    belief.ConfidenceLevel = value;
                    break;
                case BeliefField.EstimatedPower:
                    belief.EstimatedPower = value;
                    break;
            }
        }

        public void ModifyBeliefField(Player player, NPC npc, BeliefField field, float delta)
        {
            var belief = GetBelief(player, npc);
            if (belief == null) return;

            switch (field)
            {
                case BeliefField.RiskThreshold:
                    belief.RiskThreshold = MathHelper.Clamp(belief.RiskThreshold + delta, 0f, 1f);
                    break;
                case BeliefField.ConfidenceLevel:
                    belief.ConfidenceLevel = MathHelper.Clamp(belief.ConfidenceLevel + delta, 0f, 1f);
                    break;
                case BeliefField.EstimatedPower:
                    belief.EstimatedPower = MathHelper.Clamp(belief.EstimatedPower + delta, 0f, 1f);
                    break;
            }
        }

        // ===== 声望系统 =====

        public int GetReputation(Player player, FactionID faction)
        {
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
            if (worldPlayer.FactionRelations.TryGetValue(faction, out var rel))
                return rel.ReputationPoints;
            return 0;
        }

        public void AddReputation(Player player, FactionID faction, int amount, string reason)
        {
            player.GetModPlayer<GuWorldPlayer>().AddReputation(faction, amount, reason);
        }

        public void RemoveReputation(Player player, FactionID faction, int amount, string reason)
        {
            player.GetModPlayer<GuWorldPlayer>().RemoveReputation(faction, amount, reason);
        }

        // ===== 修为系统 =====

        public int GetGuLevel(Player player)
        {
            return player.GetModPlayer<QiRealmPlayer>().GuLevel;
        }

        // ===== 物品系统 =====

        public bool HasItem(Player player, int itemType, int minStack)
        {
            return player.HasItem(itemType) && player.CountItem(itemType) >= minStack;
        }

        public void GiveItem(Player player, int itemType, int stack)
        {
            player.QuickSpawnItem(new EntitySource_DebugCommand("DialogueTree"), itemType, stack);
        }

        public bool RemoveItem(Player player, int itemType, int stack)
        {
            int remaining = stack;
            for (int i = 0; i < player.inventory.Length && remaining > 0; i++)
            {
                var item = player.inventory[i];
                if (item.type == itemType && item.stack > 0)
                {
                    int take = Math.Min(item.stack, remaining);
                    item.stack -= take;
                    remaining -= take;
                    if (item.stack <= 0)
                        item.TurnToAir();
                }
            }
            return remaining == 0;
        }

        // ===== 态度系统 =====

        public GuAttitude GetAttitude(Player player, NPC npc)
        {
            if (npc.ModNPC is GuMasterBase guMaster)
                return guMaster.CurrentAttitude;
            return GuAttitude.Ignore;
        }

        public void SetAttitude(Player player, NPC npc, GuAttitude attitude)
        {
            if (npc.ModNPC is GuMasterBase guMaster)
                guMaster.CurrentAttitude = attitude;
        }

        // ===== 标记系统 =====

        public bool GetFlag(Player player, string flagName)
        {
            var session = DialogueTreeManager.Instance.CurrentSession;
            if (session.IsActive && session.LocalFlags.TryGetValue(flagName, out bool value))
                return value;
            return false;
        }

        public void SetFlag(Player player, string flagName, bool value)
        {
            var session = DialogueTreeManager.Instance.CurrentSession;
            if (session.IsActive)
                session.LocalFlags[flagName] = value;
        }

        // ===== 元石系统 =====

        private static int YuanSItemType => ModContent.ItemType<YuanS>();

        public int GetYuanS(Player player)
        {
            return player.CountItem(YuanSItemType);
        }

        public bool SpendYuanS(Player player, int amount)
        {
            int held = player.CountItem(YuanSItemType);
            if (held < amount)
                return false;
            for (int i = 0; i < player.inventory.Length && amount > 0; i++)
            {
                var item = player.inventory[i];
                if (item.type == YuanSItemType && item.stack > 0)
                {
                    int take = System.Math.Min(item.stack, amount);
                    item.stack -= take;
                    amount -= take;
                    if (item.stack <= 0)
                        item.TurnToAir();
                }
            }
            return true;
        }

        public void GiveYuanS(Player player, int amount)
        {
            player.QuickSpawnItem(new EntitySource_DebugCommand("DialogueTree"), YuanSItemType, amount);
        }
    }
}