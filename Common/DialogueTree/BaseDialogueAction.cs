using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.DialogueTree
{
    /// <summary>
    /// 通用动作基类 — 封装通用逻辑，子类只需实现 ExecuteCore。
    /// </summary>
    public abstract class BaseDialogueAction : IDialogueAction
    {
        public abstract string ActionID { get; }
        public abstract string DisplayName { get; }
        public abstract string Description { get; }
        public abstract DialogueOptionType OptionType { get; }

        /// <summary> 最低修为要求（0 表示无要求） </summary>
        protected virtual int MinRealm => 0;

        /// <summary> 风险等级（0=安全, 1=低风险, 2=中风险, 3=高风险） </summary>
        protected virtual int RiskLevel => 0;

        /// <summary> 是否要求 NPC 有信念状态 </summary>
        protected virtual bool RequiresBelief => false;

        public virtual bool IsAvailable(DialogueActionContext context)
        {
            if (MinRealm > 0)
            {
                int playerLevel = context.Integration?.GetGuLevel(context.Player) ?? 0;
                if (playerLevel < MinRealm)
                    return false;
            }

            if (RequiresBelief && context.Belief == null)
                return false;

            return true;
        }

        public DialogueActionResult Execute(DialogueActionContext context)
        {
            return ExecuteCore(context);
        }

        protected abstract DialogueActionResult ExecuteCore(DialogueActionContext context);

        /// <summary>
        /// 根据风险等级和信念状态计算成功率
        /// </summary>
        protected float CalculateSuccessRate(DialogueActionContext context)
        {
            float baseRate = 1.0f - (RiskLevel * 0.2f);

            if (context.Belief != null)
            {
                baseRate -= context.Belief.RiskThreshold * 0.3f;
                baseRate += (1f - context.Belief.ConfidenceLevel) * 0.2f;
            }

            return MathHelper.Clamp(baseRate, 0.1f, 0.95f);
        }

        /// <summary>
        /// 判定是否成功
        /// </summary>
        protected bool RollSuccess(DialogueActionContext context)
        {
            float rate = CalculateSuccessRate(context);
            return Main.rand.NextFloat() < rate;
        }
    }
}