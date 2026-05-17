using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 战术Buff激活状态 — 记录当前生效的战术增益
    /// </summary>
    public struct ActiveTacticalBuff
    {
        /// <summary>触发类型（连击/背击/击杀/受击等）</summary>
        public TacticalTrigger Trigger;

        /// <summary>剩余持续帧数</summary>
        public int RemainingFrames;

        /// <summary>伤害倍率加成</summary>
        public float DamageMultiplier;

        /// <summary>速度倍率加成</summary>
        public float SpeedMultiplier;

        /// <summary>暴击率加成</summary>
        public float CritBonus;

        /// <summary>吸血率加成</summary>
        public float LifeStealBonus;

        /// <summary>防御力加成</summary>
        public float DefenseBonus;
    }

    /// <summary>
    /// 战术触发提供者接口 — 武器实现此接口以定义战术触发条件。
    /// 由 TacticalTriggerSystem 在命中时调用 CheckTriggers 获取武器定义的战术Buff。
    /// </summary>
    public interface ITacticalTriggerProvider
    {
        TacticalTrigger[] Triggers { get; }
        void CheckTriggers(Player player, out List<ActiveTacticalBuff> activeBuffs);
    }
}
