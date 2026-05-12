using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.DialogueTree.Actions
{
    /// <summary>
    /// 偷窃 — 从 NPC 身上偷取物品。
    /// 高风险，成功则获得随机物品，失败则触发战斗或声望大幅下降。
    /// </summary>
    public class StealAction : BaseDialogueAction
    {
        public override string ActionID => "steal";
        public override string DisplayName => "偷窃";
        public override string Description => "趁其不备，偷取对方身上的财物（高风险）";
        public override DialogueOptionType OptionType => DialogueOptionType.Steal;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 3;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.AddReputation(context.Player, FactionID.Scattered, -30, "偷窃失败");
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.3f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.3f);

                return DialogueActionResult.CombatResult(
                    "\"好大的胆子！敢偷到你爷爷头上！\"");
            }

            int stolenItem = Main.rand.Next(new[] {
                ItemID.GoldCoin, ItemID.SilverCoin,
                ItemID.LesserHealingPotion, ItemID.IronBar
            });
            int stack = Main.rand.Next(1, 4);
            context.Integration.GiveItem(context.Player, stolenItem, stack);

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.2f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.2f);

            string itemName = Lang.GetItemNameValue(stolenItem);
            return DialogueActionResult.SuccessResult(
                $"你悄悄摸走了 {stack} 个{itemName}，对方似乎并未察觉。");
        }
    }

    /// <summary>
    /// 背刺 — 直接攻击 NPC。
    /// 极高风险，必定触发战斗，但可获得先手优势。
    /// </summary>
    public class BackstabAction : BaseDialogueAction
    {
        public override string ActionID => "backstab";
        public override string DisplayName => "背刺";
        public override string Description => "趁对方不备，突然出手攻击（必定触发战斗）";
        public override DialogueOptionType OptionType => DialogueOptionType.Betray;

        protected override int MinRealm => 3;
        protected override int RiskLevel => 3;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            context.Integration.AddReputation(context.Player, FactionID.Scattered, -50, "背刺");
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.4f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.5f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.EstimatedPower, 0.2f);

            return DialogueActionResult.CombatResult(
                "\"你……！卑鄙！\"");
        }
    }

    /// <summary>
    /// 挑衅 — 用言语激怒对方。
    /// 中风险，成功则降低对方风险阈值（使其更易冲动），失败则直接触发战斗。
    /// </summary>
    public class ProvokeAction : BaseDialogueAction
    {
        public override string ActionID => "provoke";
        public override string DisplayName => "挑衅";
        public override string Description => "用言语激怒对方，使其失去理智";
        public override DialogueOptionType OptionType => DialogueOptionType.Risky;

        protected override int MinRealm => 1;
        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, -0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.1f);

                return DialogueActionResult.SuccessResult(
                    "对方被你激得面红耳赤，呼吸急促起来。");
            }

            context.Integration.AddReputation(context.Player, FactionID.Scattered, -15, "挑衅失败");
            return DialogueActionResult.CombatResult(
                "\"找死！\"");
        }
    }

    /// <summary>
    /// 下毒 — 暗中在对方的饮食或物品中下毒。
    /// 高风险，成功则削弱对方战力，失败则暴露并被追杀。
    /// </summary>
    public class PoisonAction : BaseDialogueAction
    {
        public override string ActionID => "poison";
        public override string DisplayName => "下毒";
        public override string Description => "暗中在对方身上下毒（高风险）";
        public override DialogueOptionType OptionType => DialogueOptionType.Deceive;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 3;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.AddReputation(context.Player, FactionID.Scattered, -40, "下毒败露");
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.4f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.4f);

                return DialogueActionResult.CombatResult(
                    "\"你……你在酒里放了什么？！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.EstimatedPower, -0.3f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.1f);

            return DialogueActionResult.SuccessResult(
                "你神不知鬼不觉地下了毒。对方毫无察觉地饮下。");
        }
    }

    /// <summary>
    /// 勒索 — 抓住对方的把柄进行勒索。
    /// 中风险，成功则获得大量元石，失败则声望大幅下降。
    /// </summary>
    public class BlackmailAction : BaseDialogueAction
    {
        public override string ActionID => "blackmail";
        public override string DisplayName => "勒索";
        public override string Description => "抓住对方的把柄进行勒索";
        public override DialogueOptionType OptionType => DialogueOptionType.Betray;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.AddReputation(context.Player, FactionID.Scattered, -35, "勒索失败");
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.3f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.3f);

                return DialogueActionResult.FailResult(
                    "\"呵，你尽管去说，看有人信你么？\"");
            }

            int ransom = Main.rand.Next(50, 151);
            context.Integration.GiveYuanS(context.Player, ransom);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.2f);

            return DialogueActionResult.SuccessResult(
                $"对方咬牙切齿地掏出 {ransom} 块元石塞给你。\"拿着，滚！\"");
        }
    }

    /// <summary>
    /// 栽赃 — 将赃物或罪证塞给对方，陷害对方。
    /// 高风险，成功则降低对方在所有势力的声望。
    /// </summary>
    public class FrameAction : BaseDialogueAction
    {
        public override string ActionID => "frame";
        public override string DisplayName => "栽赃";
        public override string Description => "将赃物塞给对方，陷害对方（高风险）";
        public override DialogueOptionType OptionType => DialogueOptionType.Deceive;

        protected override int MinRealm => 3;
        protected override int RiskLevel => 3;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.AddReputation(context.Player, FactionID.Scattered, -50, "栽赃败露");
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.4f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.5f);

                return DialogueActionResult.CombatResult(
                    "\"好哇，敢陷害到你爷爷头上！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.2f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.1f);

            return DialogueActionResult.SuccessResult(
                "你悄悄将赃物塞入对方行囊。很快，周围的人开始用异样的眼光看向他……");
        }
    }

    /// <summary>
    /// 威吓 — 以武力或势力威胁对方。
    /// 中风险，成功则迫使对方配合，失败则触发战斗。
    /// </summary>
    public class IntimidateAction : BaseDialogueAction
    {
        public override string ActionID => "intimidate";
        public override string DisplayName => "威吓";
        public override string Description => "以武力威胁对方就范";
        public override DialogueOptionType OptionType => DialogueOptionType.Risky;

        protected override int MinRealm => 3;
        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.EstimatedPower, -0.15f);

                return DialogueActionResult.CombatResult(
                    "\"吓唬谁呢！手底下见真章！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.2f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.EstimatedPower, 0.15f);

            return DialogueActionResult.SuccessResult(
                "你冷哼一声，气势外放。对方脸色发白，冷汗涔涔。");
        }
    }
}