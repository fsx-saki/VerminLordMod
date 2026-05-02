using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 汇聚弹幕基类 — 抽象出 BloodWaterProj 的追踪汇聚模式。
    ///
    /// 用于"从 A 点飞向 B 点并汇聚"的弹幕，例如：
    /// - 血手印蓄力时的血液汇聚弹幕（BloodWaterProj）
    /// - 能量汇聚弹幕
    /// - 粒子汇聚弹幕
    ///
    /// 使用方式：
    /// <code>
    /// class MyConvergeProj : ConvergeProjectile
    /// {
    ///     protected override int TargetProjType => ModContent.ProjectileType<MyTargetProj>();
    ///     protected override float ConvergeDistance => 20f;
    ///     protected override float MinSpeed => 4f;
    ///     protected override float MaxSpeed => 12f;
    ///     protected override float LerpFactor => 0.08f;
    ///     protected override float FreeFlightDrag => 0.98f;
    ///     protected override float FreeFlightKillSpeed => 0.5f;
    /// }
    /// </code>
    ///
    /// 核心机制：
    /// - 通过 ai[0] 存储目标弹幕索引（TargetProjIndex 属性）
    /// - 追踪模式下：飞向目标，到达 ConvergeDistance 后 Kill
    /// - 自由飞行模式（目标不存在或 TargetProjIndex < 0）：减速直至停止
    /// </summary>
    public abstract class ConvergeProjectile : BaseBullet
    {
        // ===== 子类必须重写的参数 =====

        /// <summary>目标弹幕的类型（用于验证目标是否匹配）</summary>
        protected abstract int TargetProjType { get; }

        // ===== 可选重写的参数 =====

        /// <summary>到达目标多近距离时视为"汇聚完成"并 Kill（默认 20）</summary>
        protected virtual float ConvergeDistance => 20f;

        /// <summary>追踪最小速度（默认 4）</summary>
        protected virtual float MinSpeed => 4f;

        /// <summary>追踪最大速度（默认 12）</summary>
        protected virtual float MaxSpeed => 12f;

        /// <summary>速度插值因子（默认 0.08，越大追踪越硬）</summary>
        protected virtual float LerpFactor => 0.08f;

        /// <summary>自由飞行时的阻力系数（默认 0.98）</summary>
        protected virtual float FreeFlightDrag => 0.98f;

        /// <summary>自由飞行时速度低于此值则 Kill（默认 0.5）</summary>
        protected virtual float FreeFlightKillSpeed => 0.5f;

        /// <summary>追踪范围上限（超过此距离不加速，默认 800）</summary>
        protected virtual float MaxTrackRange => 800f;

        // ===== 公开属性 =====

        /// <summary>目标弹幕的索引</summary>
        public int TargetProjIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // ===== 生命周期 =====

        protected sealed override void OnAI()
        {
            if (TargetProjIndex >= 0 && TargetProjIndex < Main.maxProjectiles)
            {
                Projectile target = Main.projectile[TargetProjIndex];
                if (target.active && target.type == TargetProjType)
                {
                    // 追踪模式：飞向目标
                    UpdateTracking(target);
                }
                else
                {
                    // 目标不存在 → 自由飞散
                    UpdateFreeFlight();
                }
            }
            else
            {
                // 没有目标（TargetProjIndex < 0）→ 自由飞散
                UpdateFreeFlight();
            }
        }

        /// <summary>追踪模式更新（可重写自定义追踪逻辑）</summary>
        protected virtual void UpdateTracking(Projectile target)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float dist = toTarget.Length();

            if (dist < ConvergeDistance)
            {
                // 到达目标 → 消失
                OnConverge(target);
                Projectile.Kill();
                return;
            }

            // 飞向目标，速度随距离变化
            float speed = MathHelper.Lerp(MinSpeed, MaxSpeed, 1f - dist / MaxTrackRange);
            toTarget.Normalize();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * speed, LerpFactor);
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        /// <summary>自由飞行模式更新（可重写）</summary>
        protected virtual void UpdateFreeFlight()
        {
            Projectile.velocity *= FreeFlightDrag;
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Length() < FreeFlightKillSpeed)
                Projectile.Kill();
        }

        /// <summary>
        /// 到达目标时调用（在 Kill 之前）。
        /// 可用于触发汇聚特效、生成粒子等。
        /// </summary>
        protected virtual void OnConverge(Projectile target) { }
    }
}
