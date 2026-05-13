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
        /// 使用 DialogueTree 系统实现多轮交互。
        /// </summary>
        private void HandleDarkDeal(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            var tree = BuildDarkDealTree(guMaster, belief);
            DialogueTreeManager.Instance.RegisterTree(tree);
            DialogueTreeManager.Instance.StartDialogue(npc, player);
        }

        private static DialogueTree.DialogueTree BuildDarkDealTree(GuMasterBase guMaster, BeliefState belief)
        {
            var treeID = $"dark_deal_{guMaster.NPC.whoAmI}";
            var b = new DialogueTreeBuilder(treeID, "root");

            b.StartNode("root",
                $"{guMaster.GuMasterDisplayName}压低声音：\"你想做什么交易？这里没有别人。\"");

            b.AddOption("购买情报", "buy_intel", DialogueOptionType.Informative,
                tooltip: "花费元石购买关于其他NPC或区域的情报");

            b.AddOption("雇佣刺杀", "hire_assassin", DialogueOptionType.Risky,
                tooltip: "花费元石雇佣NPC去刺杀目标");

            b.AddOption("背叛交易", "betrayal_deal", DialogueOptionType.Betray,
                tooltip: "策反NPC背叛其所属势力");

            b.AddOption("算了", "exit_deal", DialogueOptionType.Exit,
                tooltip: "退出暗面交易");

            b.StartNode("buy_intel",
                $"{guMaster.GuMasterDisplayName}环顾四周：\"你想知道什么？\"")
                .AddOptionWithEffects("打听其他蛊师的情报", "intel_gu_master",
                    DialogueOptionType.Informative, null, "花费 20 元石获取蛊师情报",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 20),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.05f),
                    new ShowMessageEffect("获得了关于附近蛊师的情报。", Color.LightBlue))
                .AddOptionWithEffects("打听区域情报", "intel_region",
                    DialogueOptionType.Informative, null, "花费 15 元石获取区域情报",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 15),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.03f),
                    new ShowMessageEffect("获得了关于附近区域的情报。", Color.LightBlue))
                .AddOption("算了，不买了", "root", DialogueOptionType.Exit);

            b.StartNode("intel_gu_master",
                $"{guMaster.GuMasterDisplayName}凑近你耳边，低声说了几句。\"记住，这些情报出了这个门我就不认。\"")
                .EndsDialogue()
                .WithEnterEffect(new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.05f));

            b.StartNode("intel_region",
                $"{guMaster.GuMasterDisplayName}在地图上指了几个位置。\"这些地方最近不太平，小心点。\"")
                .EndsDialogue()
                .WithEnterEffect(new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                    ModifyBeliefEffect.ModifyOp.Add, -0.03f));

            b.StartNode("hire_assassin",
                $"{guMaster.GuMasterDisplayName}眼中闪过一丝寒光：\"你想让谁消失？\"")
                .AddOptionWithEffects("雇佣刺杀敌对蛊师（50元石）", "assassin_confirm",
                    DialogueOptionType.Combat, null, "花费 50 元石，NPC将尝试刺杀目标",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 50),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.1f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.1f),
                    new ShowMessageEffect("刺杀委托已下达，等待结果...", Color.OrangeRed))
                .AddOption("还是算了", "root", DialogueOptionType.Exit);

            b.StartNode("assassin_confirm",
                $"{guMaster.GuMasterDisplayName}收起元石：\"三天之内，你会听到好消息。\"")
                .EndsDialogue();

            b.StartNode("betrayal_deal",
                $"{guMaster.GuMasterDisplayName}沉默了片刻：\"你想让我背叛家族？\"")
                .AddOptionWithEffects("策反NPC加入你的阵营（100元石）", "betrayal_confirm",
                    DialogueOptionType.Betray, null, "花费 100 元石策反NPC",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 100),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.2f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.2f),
                    new ShowMessageEffect("策反成功！NPC已成为你的内应。", Color.Gold))
                .AddOption("开个玩笑而已", "root", DialogueOptionType.Exit);

            b.StartNode("betrayal_confirm",
                $"{guMaster.GuMasterDisplayName}深吸一口气：\"好，从今天起，我为你效力。但若你负我...\"")
                .EndsDialogue();

            b.StartNode("exit_deal",
                $"{guMaster.GuMasterDisplayName}恢复了正常的表情：\"那就下次再说。\"")
                .EndsDialogue();

            return b.Build();
        }

        // ============================================================
        // 第三层：杀招准备（D-21）
        // ============================================================

        /// <summary>
        /// 杀招准备：下毒、设陷阱、策反。
        /// 置信度 > 0.7 + 私下相处时解锁。
        /// 使用 DialogueTree 系统实现多轮交互。
        /// </summary>
        private void HandleKillPrep(Player player, NPC npc, GuMasterBase guMaster)
        {
            var belief = guMaster.GetBelief(player.name);
            if (belief == null) return;

            var tree = BuildKillPrepTree(guMaster, belief);
            DialogueTreeManager.Instance.RegisterTree(tree);
            DialogueTreeManager.Instance.StartDialogue(npc, player);

            var tacticalPlayer = player.GetModPlayer<TacticalTriggerPlayer>();
            tacticalPlayer.ActivatePerfectDodgeWindow();
        }

        private static DialogueTree.DialogueTree BuildKillPrepTree(GuMasterBase guMaster, BeliefState belief)
        {
            var treeID = $"kill_prep_{guMaster.NPC.whoAmI}";
            var b = new DialogueTreeBuilder(treeID, "root");

            b.StartNode("root",
                $"{guMaster.GuMasterDisplayName}信任地看着你：\"有什么需要我帮忙的？这里很安全。\"");

            b.AddOption("下毒", "poison", DialogueOptionType.Risky,
                tooltip: "对目标 NPC 下毒，削弱其实力");

            b.AddOption("设陷阱", "trap", DialogueOptionType.Risky,
                tooltip: "在目标区域设陷阱，伏击敌人");

            b.AddOption("策反", "subvert", DialogueOptionType.Betray,
                tooltip: "策反目标 NPC 背叛其阵营");

            b.AddOption("取消", "exit_prep", DialogueOptionType.Exit,
                tooltip: "退出杀招准备");

            b.StartNode("poison",
                $"{guMaster.GuMasterDisplayName}从怀中取出一个小瓶：\"这是我珍藏的蛊毒，无色无味。\"")
                .AddOptionWithEffects("对敌对蛊师下毒（30元石）", "poison_confirm",
                    DialogueOptionType.Risky, null, "花费 30 元石，削弱目标蛊师",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 30),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.08f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.1f),
                    new ShowMessageEffect("蛊毒已投放，目标将在数日内虚弱。", Color.DarkGreen))
                .AddOptionWithEffects("对野兽下毒（10元石）", "poison_beast",
                    DialogueOptionType.Risky, null, "花费 10 元石，削弱区域野兽",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 10),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.03f),
                    new ShowMessageEffect("毒饵已投放，区域野兽将被削弱。", Color.DarkGreen))
                .AddOption("还是算了", "root", DialogueOptionType.Exit);

            b.StartNode("poison_confirm",
                $"{guMaster.GuMasterDisplayName}阴冷地笑了笑：\"蛊毒已经布下，等着看好戏吧。\"")
                .EndsDialogue();

            b.StartNode("poison_beast",
                $"{guMaster.GuMasterDisplayName}点点头：\"小事一桩，毒饵已经撒好了。\"")
                .EndsDialogue();

            b.StartNode("trap",
                $"{guMaster.GuMasterDisplayName}眼中闪过精光：\"你想在哪里设伏？\"")
                .AddOptionWithEffects("在野外设陷阱（25元石）", "trap_wild",
                    DialogueOptionType.Risky, null, "花费 25 元石，在野外区域设陷阱",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 25),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.06f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.08f),
                    new ShowMessageEffect("陷阱已布置，经过的敌人将受到伤害。", Color.Orange))
                .AddOptionWithEffects("在山寨附近设陷阱（40元石）", "trap_village",
                    DialogueOptionType.Risky, null, "花费 40 元石，在山寨附近设陷阱",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 40),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.1f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.12f),
                    new ShowMessageEffect("山寨附近的陷阱已布置完毕。", Color.Orange))
                .AddOption("不设了", "root", DialogueOptionType.Exit);

            b.StartNode("trap_wild",
                $"{guMaster.GuMasterDisplayName}拍了拍手上的土：\"搞定了，谁踩谁倒霉。\"")
                .EndsDialogue();

            b.StartNode("trap_village",
                $"{guMaster.GuMasterDisplayName}谨慎地看了看四周：\"山寨附近的陷阱已经布好，但千万别让人发现是我干的。\"")
                .EndsDialogue();

            b.StartNode("subvert",
                $"{guMaster.GuMasterDisplayName}沉思片刻：\"策反可不是小事，你想策反谁？\"")
                .AddOptionWithEffects("策反敌对蛊师（80元石）", "subvert_enemy",
                    DialogueOptionType.Betray, null, "花费 80 元石策反敌对蛊师",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 80),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.15f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.15f),
                    new ShowMessageEffect("策反行动已开始，目标蛊师将逐渐倒向你。", Color.Gold))
                .AddOptionWithEffects("策反山寨守卫（120元石）", "subvert_guard",
                    DialogueOptionType.Betray, null, "花费 120 元石策反山寨守卫",
                    new BuyItemEffect(ModContent.ItemType<Content.Items.Consumables.YuanS>(), 120),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.ConfidenceLevel,
                        ModifyBeliefEffect.ModifyOp.Add, 0.2f),
                    new ModifyBeliefEffect(ModifyBeliefEffect.BeliefField.RiskThreshold,
                        ModifyBeliefEffect.ModifyOp.Add, -0.2f),
                    new ShowMessageEffect("守卫已被策反，山寨防御出现漏洞。", Color.Gold))
                .AddOption("风险太大，算了", "root", DialogueOptionType.Exit);

            b.StartNode("subvert_enemy",
                $"{guMaster.GuMasterDisplayName}露出意味深长的笑容：\"敌人的敌人就是朋友，我明白了。\"")
                .EndsDialogue();

            b.StartNode("subvert_guard",
                $"{guMaster.GuMasterDisplayName}压低声音：\"守卫那边我已经打过招呼了，今晚子时，东门无人值守。\"")
                .EndsDialogue();

            b.StartNode("exit_prep",
                $"{guMaster.GuMasterDisplayName}恢复了常态：\"也好，谨慎行事总是没错的。\"")
                .EndsDialogue();

            return b.Build();
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
