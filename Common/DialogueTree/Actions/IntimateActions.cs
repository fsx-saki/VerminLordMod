using Terraria;
using Terraria.ID;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.DialogueTree.Actions
{
    /// <summary>
    /// 调戏 — 用言语挑逗对方。
    /// 亲密行为，成功则提升好感度，失败则引起反感。
    /// </summary>
    public class FlirtAction : BaseDialogueAction
    {
        public override string ActionID => "flirt";
        public override string DisplayName => "调戏";
        public override string Description => "用言语挑逗对方";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

        protected override int RiskLevel => 2;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.2f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.2f);

                return DialogueActionResult.FailResult(
                    "\"请自重！再这样我就不客气了！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.1f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.1f);

            string response = context.Belief.ConfidenceLevel switch
            {
                >= 0.7f => "对方脸一红，低声啐道：\"油嘴滑舌……\"但嘴角却微微上扬。",
                >= 0.4f => "对方愣了一下，有些不知所措：\"你……你说什么呢！\"",
                _ => "对方冷冷地瞥了你一眼，没有接话。"
            };

            return DialogueActionResult.SuccessResult(response);
        }
    }

    /// <summary>
    /// 拥抱 — 突然抱住对方。
    /// 亲密行为，高风险，成功则大幅提升好感度，失败则触发战斗。
    /// </summary>
    public class HugAction : BaseDialogueAction
    {
        public override string ActionID => "hug";
        public override string DisplayName => "拥抱";
        public override string Description => "突然上前抱住对方（高风险）";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

        protected override int RiskLevel => 3;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.3f);
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.ConfidenceLevel, -0.3f);

                return DialogueActionResult.CombatResult(
                    "\"放肆！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.2f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.2f);

            return DialogueActionResult.SuccessResult(
                "你一把抱住对方。对方身体一僵，但没有推开你。");
        }
    }

    /// <summary>
    /// 送礼 — 赠送礼物以增进关系。
    /// 亲密行为，低风险，消耗物品提升好感度。
    /// </summary>
    public class GiftAction : BaseDialogueAction
    {
        public override string ActionID => "gift";
        public override string DisplayName => "送礼";
        public override string Description => "赠送一件礼物以表心意（消耗一个金锭）";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!context.Integration.HasItem(context.Player, ItemID.GoldBar, 1))
            {
                return DialogueActionResult.FailResult(
                    "你翻遍全身，找不到一件拿得出手的东西。");
            }

            context.Integration.RemoveItem(context.Player, ItemID.GoldBar, 1);

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.2f);

            return DialogueActionResult.SuccessResult(
                "你递上礼物，对方犹豫了一下，还是收下了。神色缓和了许多。");
        }
    }

    /// <summary>
    /// 牵手 — 轻轻牵起对方的手。
    /// 亲密行为，中风险，成功则大幅提升好感度，失败则尴尬收场。
    /// </summary>
    public class HoldHandAction : BaseDialogueAction
    {
        public override string ActionID => "hold_hand";
        public override string DisplayName => "牵手";
        public override string Description => "轻轻牵起对方的手";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

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
                    "你伸手去牵对方的手，对方像被烫到一样缩了回去。\"别碰我！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.15f);

            string response = context.Belief.ConfidenceLevel switch
            {
                >= 0.7f => "你轻轻握住对方的手。对方微微一颤，反手握紧了你。",
                >= 0.4f => "你牵起对方的手。对方有些惊讶，但没有挣脱。",
                _ => "你碰了一下对方的手背，对方迅速抽回了手。"
            };

            return DialogueActionResult.SuccessResult(response);
        }
    }

    /// <summary>
    /// 耳语 — 在对方耳边说悄悄话。
    /// 亲密行为，低风险，成功则拉近距离。
    /// </summary>
    public class EarWhisperAction : BaseDialogueAction
    {
        public override string ActionID => "ear_whisper";
        public override string DisplayName => "耳语";
        public override string Description => "凑到对方耳边说悄悄话";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

        protected override int RiskLevel => 1;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!RollSuccess(context))
            {
                context.Integration.ModifyBeliefField(context.Player, context.NPC,
                    BeliefField.RiskThreshold, 0.1f);

                return DialogueActionResult.FailResult(
                    "\"有什么话不能当面说？鬼鬼祟祟的。\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.1f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.1f);

            string[] whispers = {
                "你在对方耳边轻声说：\"你今天很好看。\"对方耳根一下子红了。",
                "你凑近低语：\"我有个秘密要告诉你……其实我注意你很久了。\"",
                "你悄悄说：\"待会儿有空吗？想和你单独聊聊。\"对方微微点了点头。"
            };

            return DialogueActionResult.SuccessResult(
                whispers[Main.rand.Next(whispers.Length)]);
        }
    }

    /// <summary>
    /// 梳发 — 为对方整理头发。
    /// 亲密行为，低风险，成功则大幅提升好感度。
    /// </summary>
    public class CombHairAction : BaseDialogueAction
    {
        public override string ActionID => "comb_hair";
        public override string DisplayName => "梳发";
        public override string Description => "伸手为对方整理头发";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

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
                    "\"别碰我头发！\"");
            }

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.15f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.2f);

            return DialogueActionResult.SuccessResult(
                "你伸手轻轻梳理对方有些凌乱的发丝。对方愣了一下，随即放松下来，似乎很享受。");
        }
    }

    /// <summary>
    /// 共饮 — 邀请对方一起喝酒。
    /// 亲密行为，低风险，消耗物品提升好感度。
    /// </summary>
    public class ShareDrinkAction : BaseDialogueAction
    {
        public override string ActionID => "share_drink";
        public override string DisplayName => "共饮";
        public override string Description => "邀请对方一起喝一杯（消耗一个瓶子）";
        public override DialogueOptionType OptionType => DialogueOptionType.Social;

        protected override int RiskLevel => 0;
        protected override bool RequiresBelief => true;

        protected override DialogueActionResult ExecuteCore(DialogueActionContext context)
        {
            if (!context.Integration.HasItem(context.Player, ItemID.Bottle, 1))
            {
                return DialogueActionResult.FailResult(
                    "你想请对方喝酒，但翻遍背包也找不到一瓶酒。");
            }

            context.Integration.RemoveItem(context.Player, ItemID.Bottle, 1);

            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.RiskThreshold, -0.1f);
            context.Integration.ModifyBeliefField(context.Player, context.NPC,
                BeliefField.ConfidenceLevel, 0.15f);

            return DialogueActionResult.SuccessResult(
                "你们对饮了一杯。酒过三巡，气氛融洽了许多，对方的话也多了起来。");
        }
    }
}