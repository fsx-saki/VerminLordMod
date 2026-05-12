using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.DialogueTree.Actions
{
    /// <summary>
    /// 贿赂 — 用元石收买 NPC。
    /// 低风险，成功则提升好感度，失败则降低声望。
    /// </summary>
    public class BribeAction : BaseDialogueAction
    {
        public override string ActionID => "bribe";
        public override string DisplayName => "贿赂";
        public override string Description => "用元石收买对方（消耗 100 元石）";
        public override DialogueOptionType OptionType => DialogueOptionType.Barter;

        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (context.Integration == null || !context.Integration.SpendYuanS(context.Player, 100))
            {
                return DialogueActionResult.FailResult(
                    "你摸了摸口袋……囊中羞涩。");
            }

            if (RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, -0.1f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, 0.15f);

                return DialogueActionResult.SuccessResult(
                    "对方收下元石，神色缓和了不少。");
            }

            context.Integration.AddReputation(context.Player, FactionID.Scattered, -10, "贿赂失败");
            return DialogueActionResult.FailResult(
                "\"你当我是什么人？收起你的臭钱！\"");
        }
    }

    /// <summary>
    /// 探查 — 观察对方的修为和状态。
    /// 安全动作，获取情报。
    /// </summary>
    public class InspectAction : BaseDialogueAction
    {
        public override string ActionID => "inspect";
        public override string DisplayName => "探查";
        public override string Description => "暗中观察对方的修为和状态";
        public override DialogueOptionType OptionType => DialogueOptionType.Informative;

        protected override int MinRealm => 1;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            string powerDesc = context.Belief?.EstimatedPower switch
            {
                >= 0.8f => "深不可测",
                >= 0.5f => "不容小觑",
                >= 0.3f => "平平无奇",
                _ => "不值一提"
            };

            string attitudeDesc = context.Belief?.ConfidenceLevel switch
            {
                >= 0.7f => "对你颇为信任",
                >= 0.4f => "态度中立",
                _ => "对你心存戒备"
            };

            return DialogueActionResult.SuccessResult(
                $"你暗中观察了一番：对方修为{powerDesc}，{attitudeDesc}。");
        }
    }

    /// <summary>
    /// 治疗 — 消耗自身真元为 NPC 疗伤。
    /// 安全动作，大幅提升好感度和信任度。
    /// </summary>
    public class HealAction : BaseDialogueAction
    {
        public override string ActionID => "heal";
        public override string DisplayName => "治疗";
        public override string Description => "消耗真元为对方疗伤（消耗 50 真元）";
        public override DialogueOptionType OptionType => DialogueOptionType.Teach;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.2f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.25f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.EstimatedPower, 0.1f);

            return DialogueActionResult.SuccessResult(
                "你运功将真元渡入对方体内。对方精神一振，面露感激之色。");
        }
    }

    /// <summary>
    /// 指路 — 为 NPC 提供情报或指引。
    /// 安全动作，提升声望和信任度。
    /// </summary>
    public class GuideAction : BaseDialogueAction
    {
        public override string ActionID => "guide";
        public override string DisplayName => "指路";
        public override string Description => "为对方提供有用的情报或指引";
        public override DialogueOptionType OptionType => DialogueOptionType.Informative;

        protected override int RiskLevel => 0;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.05f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.1f);

            return DialogueActionResult.SuccessResult(
                "你将所知的情报详细告知了对方。对方连连点头，\"多谢指点！\"");
        }
    }

    /// <summary>
    /// 担保 — 以自己的声誉为某人担保。
    /// 中风险，消耗声望换取对方的信任。
    /// </summary>
    public class VouchAction : BaseDialogueAction
    {
        public override string ActionID => "vouch";
        public override string DisplayName => "担保";
        public override string Description => "以自己的声誉为某人担保（消耗声望）";
        public override DialogueOptionType OptionType => DialogueOptionType.Ally;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            int currentRep = context.Integration.GetReputation(context.Player, FactionID.Scattered);
            if (currentRep < 50)
            {
                return DialogueActionResult.FailResult(
                    "你掂量了一下自己的分量……还是别丢人现眼了。");
            }

            context.Integration.AddReputation(context.Player, FactionID.Scattered, -30, "担保消耗");
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.2f);

            return DialogueActionResult.SuccessResult(
                "\"既然有你担保，那便信他一次。\"");
        }
    }
}