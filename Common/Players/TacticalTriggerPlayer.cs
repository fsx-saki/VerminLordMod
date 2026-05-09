using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 战术触发状态追踪器（D-29）
    /// 
    /// 追踪需要跨帧/跨事件维护的触发条件状态：
    /// - ComboCount / ComboTimer：连击计数
    /// - HitTakenCount：受击计数
    /// - LastBackstabTime：背击标记
    /// - PerfectDodgeWindow：完美闪避窗口
    /// - AllyDeathFlag：队友死亡标记
    /// </summary>
    public class TacticalTriggerPlayer : ModPlayer
    {
        // ===== 连击追踪 =====
        /// <summary>当前连击数</summary>
        public int ComboCount = 0;
        /// <summary>连击超时计时器（帧），超过此值重置连击</summary>
        public int ComboTimer = 0;
        private const int ComboTimeout = 120; // 2秒（60fps）

        // ===== 受击追踪 =====
        /// <summary>连续受击计数</summary>
        public int HitTakenCount = 0;
        /// <summary>受击计数重置计时器</summary>
        public int HitTakenTimer = 0;
        private const int HitTakenTimeout = 300; // 5秒

        // ===== 背击追踪 =====
        /// <summary>上次背击时间戳（GameUpdateCount）</summary>
        public ulong LastBackstabFrame = 0;
        private const ulong BackstabWindow = 10; // 背击标记持续10帧

        // ===== 完美闪避追踪 =====
        /// <summary>完美闪避窗口激活</summary>
        public bool PerfectDodgeWindowActive = false;
        /// <summary>完美闪避窗口计时器</summary>
        public int PerfectDodgeTimer = 0;
        private const int PerfectDodgeWindowFrames = 15; // 0.25秒窗口

        // ===== 队友死亡标记 =====
        /// <summary>队友死亡标记（持续到下次攻击）</summary>
        public bool AllyDeathFlag = false;

        // ===== 当前激活的战术Buff =====
        /// <summary>由武器 CheckTriggers 产生的激活Buff列表</summary>
        public List<ActiveTacticalBuff> ActiveBuffs = new();

        public override void ResetEffects()
        {
            // 连击超时重置
            if (ComboCount > 0)
            {
                ComboTimer--;
                if (ComboTimer <= 0)
                {
                    ComboCount = 0;
                }
            }

            // 受击计数超时重置
            if (HitTakenCount > 0)
            {
                HitTakenTimer--;
                if (HitTakenTimer <= 0)
                {
                    HitTakenCount = 0;
                }
            }

            // 完美闪避窗口超时
            if (PerfectDodgeWindowActive)
            {
                PerfectDodgeTimer--;
                if (PerfectDodgeTimer <= 0)
                {
                    PerfectDodgeWindowActive = false;
                }
            }

            // 背击标记超时（基于帧计数）
            if (LastBackstabFrame > 0 && Main.GameUpdateCount - LastBackstabFrame > BackstabWindow)
            {
                LastBackstabFrame = 0;
            }

            // 递减激活Buff的剩余帧数，移除过期的
            for (int i = ActiveBuffs.Count - 1; i >= 0; i--)
            {
                var buff = ActiveBuffs[i];
                buff.RemainingFrames--;
                if (buff.RemainingFrames <= 0)
                {
                    ActiveBuffs.RemoveAt(i);
                }
                else
                {
                    ActiveBuffs[i] = buff;
                }
            }
        }

        /// <summary>
        /// 记录一次命中，增加连击计数。
        /// </summary>
        public void RegisterHit()
        {
            ComboCount++;
            ComboTimer = ComboTimeout;
        }

        /// <summary>
        /// 记录一次受击。
        /// </summary>
        public void RegisterHitTaken()
        {
            HitTakenCount++;
            HitTakenTimer = HitTakenTimeout;
        }

        /// <summary>
        /// 检查是否为背击（NPC朝向与攻击方向相反）。
        /// </summary>
        public bool CheckBackstab(NPC target, Projectile projectile)
        {
            if (target == null || !target.active) return false;

            // 背击判断：NPC的朝向与弹幕到NPC的方向相反
            float npcDir = target.direction; // 1=右, -1=左
            float hitDir = projectile.Center.X - target.Center.X;

            bool isBackstab = (npcDir > 0 && hitDir < 0) || (npcDir < 0 && hitDir > 0);

            if (isBackstab)
            {
                LastBackstabFrame = Main.GameUpdateCount;
            }

            return isBackstab;
        }

        /// <summary>
        /// 检查是否为完美闪避（在闪避窗口内）。
        /// </summary>
        public bool CheckPerfectDodge()
        {
            return PerfectDodgeWindowActive;
        }

        /// <summary>
        /// 激活完美闪避窗口（由闪避事件触发）。
        /// </summary>
        public void ActivatePerfectDodgeWindow()
        {
            PerfectDodgeWindowActive = true;
            PerfectDodgeTimer = PerfectDodgeWindowFrames;
        }

        /// <summary>
        /// 标记队友死亡（由死亡事件触发）。
        /// </summary>
        public void MarkAllyDeath()
        {
            AllyDeathFlag = true;
        }

        /// <summary>
        /// 获取当前所有激活的战术Buff（合并武器触发 + 事件触发）。
        /// </summary>
        public List<ActiveTacticalBuff> GetActiveBuffs()
        {
            return ActiveBuffs;
        }

        /// <summary>
        /// 计算所有激活Buff的聚合加成。
        /// </summary>
        public TacticalBuffAggregate GetAggregateBuffs()
        {
            var agg = new TacticalBuffAggregate();
            foreach (var buff in ActiveBuffs)
            {
                agg.DamageMultiplier *= buff.DamageMultiplier > 0 ? buff.DamageMultiplier : 1f;
                agg.SpeedMultiplier *= buff.SpeedMultiplier > 0 ? buff.SpeedMultiplier : 1f;
                agg.CritBonus += buff.CritBonus;
                agg.LifeStealBonus += buff.LifeStealBonus;
                agg.DefenseBonus += buff.DefenseBonus;
            }
            return agg;
        }
    }

    /// <summary>
    /// 战术Buff聚合加成结果。
    /// </summary>
    public struct TacticalBuffAggregate
    {
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float CritBonus;
        public float LifeStealBonus;
        public float DefenseBonus;

        public static readonly TacticalBuffAggregate Default = new()
        {
            DamageMultiplier = 1f,
            SpeedMultiplier = 1f,
            CritBonus = 0f,
            LifeStealBonus = 0f,
            DefenseBonus = 0f,
        };
    }
}
