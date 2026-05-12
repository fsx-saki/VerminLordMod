using Terraria;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.DialogueTree.Actions
{
    /// <summary>
    /// 触摸 — 伸手触碰对方。
    /// 怪异行为，成功则获取对方的身体信息（修为、伤势等），
    /// 失败则引起对方警觉，好感度下降。
    /// </summary>
    public class TouchAction : BaseDialogueAction
    {
        public override string ActionID => "touch";
        public override string DisplayName => "触摸";
        public override string Description => "伸手触碰对方，感知其身体状况";
        public override DialogueOptionType OptionType => DialogueOptionType.Special;

        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.15f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.1f);

                return DialogueActionResult.FailResult(
                    "\"你干什么！离我远点！\"");
            }

            string healthDesc = context.Belief.EstimatedPower switch
            {
                >= 0.8f => "气血充盈，修为深厚",
                >= 0.5f => "气息平稳，状态尚可",
                >= 0.3f => "气血亏虚，似有暗伤",
                _ => "气息微弱，身受重伤"
            };

            return DialogueActionResult.SuccessResult(
                $"你借触碰之机暗中探查：对方{healthDesc}。");
        }
    }

    /// <summary>
    /// 嗅探 — 闻对方的气味。
    /// 怪异行为，成功则获取对方的气味信息（是否中毒、服用过什么药物等），
    /// 失败则引起对方反感。
    /// </summary>
    public class SniffAction : BaseDialogueAction
    {
        public override string ActionID => "sniff";
        public override string DisplayName => "嗅探";
        public override string Description => "靠近闻对方身上的气味";
        public override DialogueOptionType OptionType => DialogueOptionType.Special;

        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.15f);

                return DialogueActionResult.FailResult(
                    "\"你……你是在闻我吗？！\"");
            }

            string scentDesc = context.Belief.RiskThreshold switch
            {
                >= 0.8f => "身上有淡淡的药草味，似乎常与丹药为伴",
                >= 0.5f => "气味寻常，无甚特别",
                < 0.5f => "隐约有血腥味，此人手上恐怕不干净",
                _ => "气味清新，像是刚沐浴过"
            };

            return DialogueActionResult.SuccessResult(
                $"你悄悄嗅了嗅：{scentDesc}。");
        }
    }

    /// <summary>
    /// 凝视 — 目不转睛地盯着对方看。
    /// 怪异行为，成功则让对方感到不安（降低风险阈值），
    /// 失败则激怒对方。
    /// </summary>
    public class StareAction : BaseDialogueAction
    {
        public override string ActionID => "stare";
        public override string DisplayName => "凝视";
        public override string Description => "一言不发，死死盯着对方的眼睛";
        public override DialogueOptionType OptionType => DialogueOptionType.Risky;

        protected override int MinRealm => 1;
        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, -0.1f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.05f);

                return DialogueActionResult.SuccessResult(
                    "你死死盯着对方的眼睛。对方被你盯得心里发毛，目光开始躲闪。");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.EstimatedPower, -0.1f);

            return DialogueActionResult.FailResult(
                "\"看什么看！没见过帅哥吗？！\"");
        }
    }

    /// <summary>
    /// 绕行 — 绕着对方缓缓走圈，制造心理压力。
    /// 怪异行为，成功则降低对方自信，使其紧张不安。
    /// </summary>
    public class CircleAction : BaseDialogueAction
    {
        public override string ActionID => "circle";
        public override string DisplayName => "绕行";
        public override string Description => "绕着对方缓缓走圈，一言不发";
        public override DialogueOptionType OptionType => DialogueOptionType.Risky;

        protected override int MinRealm => 1;
        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.15f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, -0.05f);

                return DialogueActionResult.SuccessResult(
                    "你开始绕着对方缓缓踱步。对方的视线紧张地跟着你转，喉结上下滚动。");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.1f);

            return DialogueActionResult.FailResult(
                "\"你转什么呢你！头晕不晕啊！\"");
        }
    }

    /// <summary>
    /// 模仿 — 模仿对方的言行举止。
    /// 怪异行为，成功则让对方感到困惑和不安，失败则引起反感。
    /// </summary>
    public class MimicAction : BaseDialogueAction
    {
        public override string ActionID => "mimic";
        public override string DisplayName => "模仿";
        public override string Description => "模仿对方的言行举止";
        public override DialogueOptionType OptionType => DialogueOptionType.Special;

        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.1f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, -0.05f);

                return DialogueActionResult.SuccessResult(
                    "你学着对方的语气和动作重复了一遍。对方愣住了，\"你……你学我作甚？！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.1f);

            return DialogueActionResult.FailResult(
                "\"幼稚。\"");
        }
    }

    /// <summary>
    /// 收集 — 收集对方的毛发、衣物纤维等。
    /// 怪异行为，成功则获得可用于追踪或诅咒的素材，失败则引起警觉。
    /// </summary>
    public class CollectAction : BaseDialogueAction
    {
        public override string ActionID => "collect";
        public override string DisplayName => "收集";
        public override string Description => "暗中收集对方的毛发或衣物纤维";
        public override DialogueOptionType OptionType => DialogueOptionType.Steal;

        protected override int MinRealm => 2;
        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.15f);

                return DialogueActionResult.FailResult(
                    "\"你手在摸什么呢？！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, 0.1f);

            return DialogueActionResult.SuccessResult(
                "你趁对方不注意，悄悄收集到了一些毛发和皮屑。这些在懂行的人手里能派上大用场。");
        }
    }

    /// <summary>
    /// 低语 — 在对方耳边低声说一些莫名其妙的话。
    /// 怪异行为，成功则让对方心神不宁，降低专注度。
    /// </summary>
    public class WhisperAction : BaseDialogueAction
    {
        public override string ActionID => "weird_whisper";
        public override string DisplayName => "低语";
        public override string Description => "凑到对方耳边低声说些奇怪的话";
        public override DialogueOptionType OptionType => DialogueOptionType.Special;

        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.15f);

                return DialogueActionResult.FailResult(
                    "\"你靠太近了！滚开！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, -0.1f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.05f);

            string[] whispers = {
                "你凑到对方耳边，用只有两人能听到的声音说：\"影子在看着你。\"",
                "你在对方耳边低语：\"你左边第三根头发白了。\"",
                "你轻声说：\"我知道你昨晚去了哪里。\"",
                "你用阴森的语气低语：\"它就在你身后。\""
            };

            return DialogueActionResult.SuccessResult(
                whispers[Main.rand.Next(whispers.Length)]);
        }
    }
}