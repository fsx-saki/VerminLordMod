// ============================================================
// DialogueDebugger - 对话树调试面板
// 左键使用：检测鼠标悬停的NPC，显示其对话树相关数据
// 包括：信念状态、态度、性格、当前对话树节点等
// 右键使用：重置目标NPC的信念状态
// ============================================================
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.Items.Debuggers;

/// <summary>
/// 对话树调试面板
/// 左键查看NPC对话树数据，右键重置信念状态
/// </summary>
public class DialogueDebugger : ModItem
{
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
        // 检测鼠标悬停的NPC
        NPC target = GetHoveredNPC();
        if (target == null)
        {
            Main.NewText("请将鼠标对准一个NPC后使用此道具。", Color.Yellow);
            return true;
        }

        DisplayDialogueDebugInfo(target, player);
        return true;
    }

    public override bool AltFunctionUse(Player player) => true;

    /// <summary>
    /// 获取鼠标悬停的NPC
    /// </summary>
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

    /// <summary>
    /// 显示NPC的对话树调试信息
    /// </summary>
    private static void DisplayDialogueDebugInfo(NPC npc, Player player)
    {
        Main.NewText($"===== 对话树调试: {npc.TypeName} (ID:{npc.whoAmI}) =====", Color.Cyan);

        // 检查是否有已注册的对话树
        bool hasTree = DialogueTreeManager.Instance.HasTree(npc);
        Main.NewText($"已注册对话树: {(hasTree ? "是" : "否")}", hasTree ? Color.Green : Color.Red);

        if (hasTree)
        {
            var tree = DialogueTreeManager.Instance.GetTree(npc);
            if (tree != null)
            {
                Main.NewText($"对话树ID: {tree.TreeID}", Color.White);
                Main.NewText($"根节点: {tree.RootNodeID}", Color.White);
                Main.NewText($"节点总数: {tree.Nodes.Count}", Color.White);
            }
        }

        // 检查是否有活跃的对话会话
        bool hasActiveSession = DialogueTreeManager.Instance.HasActiveSession(player);
        Main.NewText($"活跃对话会话: {(hasActiveSession ? "是" : "否")}", hasActiveSession ? Color.Green : Color.Gray);

        if (hasActiveSession)
        {
            string currentNodeID = DialogueTreeManager.Instance.GetCurrentNodeID(player);
            string currentNPCText = DialogueTreeManager.Instance.GetCurrentNPCText(player);
            Main.NewText($"当前节点: {currentNodeID}", Color.White);
            Main.NewText($"NPC文本: {currentNPCText}", Color.White);

            var options = DialogueTreeManager.Instance.GetCurrentOptions(player);
            if (options != null)
            {
                Main.NewText($"可用选项数: {options.Count}", Color.White);
                for (int i = 0; i < options.Count && i < 5; i++)
                {
                    Main.NewText($"  选项{i + 1}: {options[i].Text} -> {options[i].TargetNodeID}", Color.Gray);
                }
                if (options.Count > 5)
                {
                    Main.NewText($"  ... 还有 {options.Count - 5} 个选项", Color.Gray);
                }
            }
        }

        // 如果是 GuMasterBase 类型，显示信念数据
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            Main.NewText($"--- 蛊师数据 ---", Color.Cyan);

            string personalityName = guMaster.GetPersonality() switch
            {
                GuPersonality.Aggressive => "好斗",
                GuPersonality.Cautious => "谨慎",
                GuPersonality.Greedy => "贪婪",
                GuPersonality.Proud => "傲慢",
                GuPersonality.Benevolent => "仁慈",
                _ => "未知"
            };
            Main.NewText($"性格: {personalityName}", Color.White);

            string attitudeName = guMaster.CurrentAttitude switch
            {
                GuAttitude.Ignore => "无视",
                GuAttitude.Wary => "警惕",
                GuAttitude.Hostile => "敌对",
                GuAttitude.Fearful => "恐惧",
                GuAttitude.Contemptuous => "轻蔑",
                GuAttitude.Friendly => "友好",
                GuAttitude.Respectful => "尊敬",
                _ => "未知"
            };
            Main.NewText($"当前态度: {attitudeName}", Color.White);

            // 信念数据
            string playerName = player.name;
            var belief = guMaster.GetBelief(playerName);
            Main.NewText($"--- 信念数据 (对 {playerName}) ---", Color.Cyan);
            Main.NewText($"风险阈值: {belief.RiskThreshold:F2} (0=自信, 1=恐惧)", Color.White);
            Main.NewText($"信心等级: {belief.ConfidenceLevel:F2} | 观察次数: {belief.ObservationCount}", Color.White);
            Main.NewText($"预估实力: {belief.EstimatedPower:F2}", Color.White);
            Main.NewText($"被击败过: {belief.WasDefeated} | 击败过玩家: {belief.HasDefeatedPlayer}", Color.White);
            Main.NewText($"交易过: {belief.HasTraded} | 战斗过: {belief.HasFought}", Color.White);
        }
        else if (npc.ModNPC is NPCs.DialogueTreeDemoNPC demoNPC)
        {
            Main.NewText($"--- 演示NPC数据 ---", Color.Cyan);
            Main.NewText($"调试信念 - 风险阈值: {demoNPC.DebugBelief.RiskThreshold:F2}", Color.White);
            Main.NewText($"调试信念 - 信心等级: {demoNPC.DebugBelief.ConfidenceLevel:F2}", Color.White);
            Main.NewText($"调试信念 - 预估实力: {demoNPC.DebugBelief.EstimatedPower:F2}", Color.White);
            Main.NewText($"调试信念 - 观察次数: {demoNPC.DebugBelief.ObservationCount}", Color.White);
            Main.NewText($"调试信念 - 被击败: {demoNPC.DebugBelief.WasDefeated} | 击败玩家: {demoNPC.DebugBelief.HasDefeatedPlayer}", Color.White);
            Main.NewText($"调试信念 - 交易过: {demoNPC.DebugBelief.HasTraded} | 战斗过: {demoNPC.DebugBelief.HasFought}", Color.White);

            string debugAttitude = demoNPC.DebugAttitude switch
            {
                GuAttitude.Ignore => "无视",
                GuAttitude.Wary => "警惕",
                GuAttitude.Hostile => "敌对",
                GuAttitude.Fearful => "恐惧",
                GuAttitude.Contemptuous => "轻蔑",
                GuAttitude.Friendly => "友好",
                GuAttitude.Respectful => "尊敬",
                _ => "未知"
            };
            string debugPersonality = demoNPC.DebugPersonality switch
            {
                GuPersonality.Aggressive => "好斗",
                GuPersonality.Cautious => "谨慎",
                GuPersonality.Greedy => "贪婪",
                GuPersonality.Proud => "傲慢",
                GuPersonality.Benevolent => "仁慈",
                _ => "未知"
            };
            Main.NewText($"调试态度: {debugAttitude}", Color.White);
            Main.NewText($"调试性格: {debugPersonality}", Color.White);
        }
        else
        {
            Main.NewText("此NPC不是蛊师或演示NPC类型。", Color.Gray);
        }
    }

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
