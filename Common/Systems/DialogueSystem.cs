using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
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
        /// </summary>
        public void GenerateDialogueOptions(Player player, NPC npc, ref string button, ref string button2)
        {
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
            // MVA 阶段：只显示提示，P1 再实现具体功能
            if (belief.ConfidenceLevel > 0.3f)
            {
                // 将 button2 改为暗面交易（覆盖交易）
                // 注意：实际 MVA 简化版只保留公开交互
                // P1 扩展为三层菜单
            }

            // 第三层：杀招准备（私下相处 + 玩家有杀意标记时解锁）
            // MVA 阶段不实现
        }

        // ============================================================
        // 对话选择处理
        // ============================================================

        /// <summary>
        /// 处理对话选择，影响信念。
        /// 由 GuMasterBase.OnChatButtonClicked 调用。
        /// </summary>
        public void OnDialogueChoice(Player player, NPC npc, int choiceIndex)
        {
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
                case 3: // 威胁（P1 扩展）
                    HandleThreat(player, npc, guMaster);
                    break;
                case 4: // 贿赂（P1 扩展）
                    HandleBribe(player, npc, guMaster);
                    break;
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
