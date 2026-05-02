// ============================================================
// BeliefModifier - 信念修改器
// 左键使用：增加目标NPC对玩家的风险阈值（恐惧）
// 右键使用：降低目标NPC对玩家的风险阈值（自信）
// 用于测试对话树的条件分支
// ============================================================
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.Items.Debuggers;

/// <summary>
/// 信念修改器
/// 左键增加风险阈值（让NPC更恐惧），右键降低风险阈值（让NPC更自信）
/// </summary>
public class BeliefModifier : ModItem
{
    private const float Delta = 0.2f;

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.rare = ItemRarityID.Master;
        Item.maxStack = 1;
        Item.value = 100;

        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Guitar;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item1;
    }

    public override bool? UseItem(Player player)
    {
        NPC target = GetHoveredNPC();
        if (target == null)
        {
            Main.NewText("请将鼠标对准一个NPC后使用此道具。", Color.Yellow);
            return true;
        }

        // 右键（altFunctionUse == 2）降低风险阈值，左键增加
        float delta = player.altFunctionUse == 2 ? -Delta : +Delta;
        ModifyBelief(target, player, delta);
        return true;
    }

    public override bool AltFunctionUse(Player player) => true;

    private static NPC GetHoveredNPC()
    {
        Vector2 mouseWorld = Main.MouseWorld;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.getRect().Contains((int)mouseWorld.X, (int)mouseWorld.Y))
            {
                return npc;
            }
        }

        return null;
    }

    private static void ModifyBelief(NPC npc, Player player, float delta)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            string playerName = player.name;
            var belief = guMaster.GetBelief(playerName);

            // 修改风险阈值
            float oldValue = belief.RiskThreshold;
            belief.RiskThreshold = MathHelper.Clamp(belief.RiskThreshold + delta, 0f, 1f);

            Main.NewText(
                $"[信念修改] {npc.TypeName} 风险阈值: {oldValue:F2} -> {belief.RiskThreshold:F2}",
                delta > 0 ? Color.Orange : Color.Green
            );

            // 同步更新信心等级（反向变化）
            belief.ConfidenceLevel = MathHelper.Clamp(belief.ConfidenceLevel - delta * 0.5f, 0f, 1f);
            Main.NewText(
                $"[信念修改] 信心等级同步调整为: {belief.ConfidenceLevel:F2}",
                Color.Gray
            );
        }
        else if (npc.ModNPC is NPCs.DialogueTreeDemoNPC demoNPC)
        {
            float oldValue = demoNPC.DebugBelief.RiskThreshold;
            demoNPC.DebugBelief.RiskThreshold = MathHelper.Clamp(
                demoNPC.DebugBelief.RiskThreshold + delta, 0f, 1f
            );

            Main.NewText(
                $"[信念修改] 演示NPC风险阈值: {oldValue:F2} -> {demoNPC.DebugBelief.RiskThreshold:F2}",
                delta > 0 ? Color.Orange : Color.Green
            );

            // 同步信心等级
            demoNPC.DebugBelief.ConfidenceLevel = MathHelper.Clamp(
                demoNPC.DebugBelief.ConfidenceLevel - delta * 0.5f, 0f, 1f
            );
        }
        else
        {
            Main.NewText("此NPC不支持信念修改。", Color.Red);
        }
    }

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
