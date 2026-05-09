using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.DialogueTree;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// DialogueSystem — 对话树与情报战（P2 MVA 阶段）
    ///
    /// 职责：
    /// 1. 劫持 GuMasterBase.SetChatButtons，扩展为三层菜单（公开/暗面/杀招）
    /// 2. 处理对话选择，影响 NPC 对玩家的信念（RiskThreshold / ConfidenceLevel）
    /// 3. 提供情报获取接口（通过对话试探 NPC 的信念状态）
    /// 4. 集成 DialogueTreeManager，支持注册的对话树 NPC
    ///
    /// MVA 阶段：
    /// - 只实现第一层（公开交互）：询问 / 交易
    /// - 第二层（暗面操作）和第三层（杀招）留 P1 扩展
    /// - 信念影响通过 InteractionResult 结构体传递
    ///
    /// 依赖：
    /// - GuMasterBase（NPC 基类，提供 GetBelief / UpdateBelief）
    /// - GuWorldPlayer（声望系统）
    /// - EventBus（事件发布）
    /// - DialogueTreeManager（对话树系统）
    /// </summary>
    public class DialogueSystem : ModSystem
    {
        // ===== 单例访问 =====
        public static DialogueSystem Instance => ModContent.GetInstance<DialogueSystem>();

        // ===== 对话选项常量 =====
        public const string BUTTON_ASK = "询问";
        public const string BUTTON_TRADE = "交易";
        public const string BUTTON_DARK_DEAL = "暗面交易";
        public const string BUTTON_KILL_PREP = "杀招准备";

        // ============================================================
        // 对话选项生成
        // ============================================================

        /// <summary>
        /// 为指定 NPC 生成对话选项。
        /// 由 GuMasterBase.SetChatButtons 调用。
        /// 优先检查是否有注册的对话树，如果有则使用对话树系统。
        /// </summary>
        public void GenerateDialogueOptions(Player player, NPC npc, ref string button, ref string button2)
        {
            // 优先检查是否有注册的对话树
            if (DialogueTreeManager.Instance.HasTree(npc))
            {
                var options = DialogueTreeManager.Instance.GetCurrentOptions(player);
                if (options != null && options.Count > 0)
                {
                    button = options.Count > 0 ? options[0].Text : "对话";
                    button2 = options.Count > 1 ? options[1].Text : "";
                    return;
                }
            }

            if (!(npc.ModNPC is GuMasterBase guMaster))
            {
                // 非蛊师 NPC：使用原版对话
                button = "对话";
                button2 = "商店";
                return;
            }

            var belief = guMaster.GetBelief(player.name);
            if (belief == null)
            {
                button = BUTTON_ASK;
                button2 = BUTTON_TRADE;
                return;
            }

            // 第一层：公开交互（始终可见）
            button = BUTTON_ASK;
            button2 = BUTTON_TRADE;

            // 第二层：暗面操作（置信度 > 0.3 时解锁）
            // D-21: 实现第二层暗面交易
            if (belief.ConfidenceLevel > 0.3f)
            {
                button2 = BUTTON_DARK_DEAL;
            }

            // 第三层：杀招准备（置信度 > 0.7 + 私下相处时解锁）
            // D-21: 实现第三层杀招准备
            if (belief.ConfidenceLevel > 0.7f && !HasWitnesses(npc, 400f))
            {
                button = BUTTON_KILL_PREP;
            }
        }

        // ============================================================
        // 对话选择处理
        // ============================================================

        /// <summary>
        /// 处理对话选择，影响信念。
        /// 由 GuMasterBase.OnChatButtonClicked 调用。
        /// 优先处理对话树系统的选择。
        /// </summary>
        public void OnDialogueChoice(Player player, NPC npc, int choiceIndex)
        {
            // 优先处理对话树系统的选择
            if (DialogueTreeManager.Instance.HasActiveSession(player))
            {
                DialogueTreeManager.Instance.SelectOption(player, choiceIndex);
                return;
            }

            if (!(npc.ModNPC is GuMasterBase guMaster)) return;

            switch (choiceIndex)
            {
                case 0: // 诚实展示修为
                    HandleHonestReveal(player, npc, guMaster);
                    break;
                case 1: // 虚报修为
                    HandleDeception(player, npc, guMaster);
                    break;
                case 2: // 拒绝回答
                    HandleRefusal(player, npc, guMaster);
                    break;
                case 3: // 威胁
                    HandleThreat(player, npc, guMaster);
                    break;
                case 4: // 贿赂
                    HandleBribe(player, npc, guMaster);
                    break;
                case 5: // 暗面交易（D-21 第二层）
                    HandleDarkDeal(player, npc, guMaster);
                    break;
                case 6: // 杀招准备（D-21 第三层）
                    HandleKillPrep(player, npc, guMaster);
                    break;
            }
        }

        /// <summary>
        /// 开始与NPC的对话树对话。
        /// 由 NPC 的 CanChat / GetChat 调用。
        /// </summary>
        public bool StartDialogueTree(Player player, NPC npc)
        {
            if (!DialogueTreeManager.Instance.HasTree(npc))
                return false;

            DialogueTreeManager.Instance.StartDialogue(npc, player);
            return true;
        }

        /// <summary>
        /// 结束对话树对话。
        /// </summary>
        public void EndDialogueTree(Player player)
        {
            if (DialogueTreeManager.Instance.HasActiveSession(player))
            {
                DialogueTreeManager.Instance.EndDialogue(player);
            }
        }

        // ============================================================
        // 对话策略处理
        // ============================================================

        /// <summary>
        /// 诚实展示修为：降低风险阈值（NPC 觉得你坦诚），小幅提升置信度。
        /// </summary>
        private void HandleHonestReveal(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            // 诚实展示：风险阈值下降（更信任），置信度上升
            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.1f);
            belief.ConfidenceLevel = MathHelper.Min(0.9f, belief.ConfidenceLevel + 0.15f);
            belief.HasTraded = true;

            // 声望影响：小幅提升
            var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
            worldPlayer.AddReputation(guMaster.GetFaction(), 5, "诚实展示修为");

            Main.npcChatText = guMaster.GuMasterDisplayName + "微微点头：\"不错，是个坦诚之人。\"";
        }

        /// <summary>
        /// 虚报修为：小幅提升置信度（NPC 觉得你友好），但被发现后大幅下降。
        /// MVA 简化：NPC 总是相信（后续按智商判定）。
        /// </summary>
        private void HandleDeception(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            // 虚报：置信度小幅上升，风险阈值小幅下降
            belief.ConfidenceLevel = MathHelper.Min(0.9f, belief.ConfidenceLevel + 0.1f);
            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.05f);

            Main.npcChatText = guMaster.GuMasterDisplayName + "若有所思：\"原来如此...\"";
        }

        /// <summary>
        /// 拒绝回答：置信度下降（NPC 觉得你有隐瞒），风险阈值上升（更警惕）。
        /// </summary>
        private void HandleRefusal(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            // 拒绝回答：置信度下降，风险阈值上升（更警惕）
            belief.ConfidenceLevel = MathHelper.Max(0f, belief.ConfidenceLevel - 0.05f);
            belief.RiskThreshold = MathHelper.Min(1f, belief.RiskThreshold + 0.1f);

            Main.npcChatText = guMaster.GuMasterDisplayName + "眯起眼睛：\"不愿说就算了。\"";
        }

        /// <summary>
        /// 威胁：大幅降低风险阈值（NPC 感到威胁），可能触发战斗。
        /// P1 扩展。
        /// </summary>
        private void HandleThreat(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.25f);
            belief.ConfidenceLevel = MathHelper.Max(0f, belief.ConfidenceLevel - 0.1f);

            // 高傲性格的 NPC 直接反击
            if (guMaster.GetPersonality() == GuPersonality.Proud)
            {
                guMaster.HandleInteraction(npc, player, InteractionType.Provoke);
                Main.npcChatText = guMaster.GuMasterDisplayName + "冷笑一声：\"你这是在找死！\"";
            }
            else
            {
                Main.npcChatText = guMaster.GuMasterDisplayName + "警惕地后退了一步。";
            }
        }

        /// <summary>
        /// 贿赂：对贪婪性格的 NPC 有效，提升声望。
        /// P1 扩展。
        /// </summary>
        private void HandleBribe(Player player, NPC npc, GuMasterBase guMaster)
        {
            var result = guMaster.HandleInteraction(npc, player, InteractionType.Bribe);
            if (result.Success)
            {
                Main.npcChatText = result.Message;
            }
            else
            {
                Main.npcChatText = result.Message;
            }
        }

        // ============================================================
        // 第二层：暗面交易（D-21）
        // ============================================================

        /// <summary>
        /// 暗面交易：情报买卖、雇佣刺杀、背叛交易。
        /// 置信度 > 0.3 时解锁。
        /// </summary>
        private void HandleDarkDeal(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            // 暗面交易选项
            string[] dealOptions = new string[]
            {
                "购买情报",
                "雇佣刺杀",
                "背叛交易",
                "算了"
            };

            // 简化实现：使用 Main.npcChatText 显示选项
            // 完整实现需要 UI 支持（P2）
            Main.npcChatText = guMaster.GuMasterDisplayName + "压低声音：\"你想做什么交易？\"\n" +
                "1. 购买情报\n" +
                "2. 雇佣刺杀\n" +
                "3. 背叛交易\n" +
                "4. 算了";

            // 根据玩家选择处理
            // 注意：这里简化处理，实际需要多轮对话支持
            // 此处仅做信念影响
            belief.ConfidenceLevel = MathHelper.Min(1f, belief.ConfidenceLevel + 0.05f);
            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.03f);

            // 尝试获取情报
            var intelNetwork = ModContent.GetInstance<IntelligenceNetwork>();
            var intel = intelNetwork.GatherIntel(player, IntelligenceNetwork.IntelType.NPCAttitude, npc);
            if (intel != null)
            {
                Main.NewText(intel.GetSummary(), Color.LightBlue);
            }
        }

        // ============================================================
        // 第三层：杀招准备（D-21）
        // ============================================================

        /// <summary>
        /// 杀招准备：下毒、设陷阱、策反。
        /// 置信度 > 0.7 + 私下相处时解锁。
        /// </summary>
        private void HandleKillPrep(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            string[] prepOptions = new string[]
            {
                "下毒",
                "设陷阱",
                "策反",
                "取消"
            };

            Main.npcChatText = guMaster.GuMasterDisplayName + "信任地看着你：\"有什么需要我帮忙的？\"\n" +
                "1. 下毒（对目标 NPC 下毒）\n" +
                "2. 设陷阱（在目标区域设陷阱）\n" +
                "3. 策反（策反目标 NPC）\n" +
                "4. 取消";

            // 杀招准备会大幅影响信念
            belief.ConfidenceLevel = MathHelper.Min(1f, belief.ConfidenceLevel + 0.1f);
            belief.RiskThreshold = MathHelper.Max(0f, belief.RiskThreshold - 0.08f);

            // 标记玩家有杀意（供其他系统使用）
            var tacticalPlayer = player.GetModPlayer<TacticalTriggerPlayer>();
            // 杀招准备增加背刺判定窗口
            tacticalPlayer.ActivatePerfectDodgeWindow();
        }

        // ============================================================
        // 辅助方法
        // ============================================================

        /// <summary>
        /// 检查 NPC 附近是否有同势力的目击者。
        /// </summary>
        private static bool HasWitnesses(NPC npc, float range)
        {
            if (npc.ModNPC is not GuMasterBase self) return false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var other = Main.npc[i];
                if (other.active && other.whoAmI != npc.whoAmI && other.ModNPC is GuMasterBase witness)
                {
                    if (witness.GetFaction() == self.GetFaction() &&
                        Vector2.Distance(npc.Center, other.Center) < range)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // ============================================================
        // 情报获取接口
        // ============================================================

        /// <summary>
        /// 获取 NPC 对玩家的信念状态摘要（用于 UI 显示）。
        /// </summary>
        public static string GetBeliefSummary(NPC npc, Player player)
        {
            if (!(npc.ModNPC is IGuMasterAI guAI)) return "未知";

            var belief = guAI.GetBelief(player.name);
            if (belief == null) return "未知";

            string riskDesc = belief.RiskThreshold switch
            {
                < 0.3f => "危险（极低风险阈值）",
                < 0.5f => "警惕（低风险阈值）",
                < 0.7f => "观望（中等风险阈值）",
                < 0.9f => "谨慎（高风险阈值）",
                _ => "恐惧（极高风险阈值）"
            };

            string confDesc = belief.ConfidenceLevel switch
            {
                < 0.3f => "陌生",
                < 0.6f => "初识",
                < 0.8f => "熟悉",
                _ => "信任"
            };

            return $"信念状态：{confDesc} | {riskDesc} | 观察次数：{belief.ObservationCount}";
        }

        // ============================================================
        // ModSystem 生命周期
        // ============================================================

        public override void PostUpdateWorld()
        {
            // 预留扩展点：P1 可添加对话事件的时间衰减
        }
    }
}
