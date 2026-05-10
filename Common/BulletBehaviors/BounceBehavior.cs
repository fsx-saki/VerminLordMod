using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 碰撞反弹行为 — 弹幕撞到物块时反弹。
    /// 可配置最大反弹次数、反弹系数、反弹后行为。
    /// </summary>
    public class BounceBehavior : IBulletBehavior
    {
        public string Name => "Bounce";

        /// <summary>最大反弹次数</summary>
        public int MaxBounces { get; set; } = 2;

        /// <summary>反弹系数（0~1），1=完全弹性</summary>
        public float BounceFactor { get; set; } = 0.4f;

        /// <summary>反弹次数（运行时可读）</summary>
        public int BounceCount { get; private set; } = 0;

        /// <summary>达到最大反弹次数后是否销毁弹幕</summary>
        public bool KillOnMaxBounces { get; set; } = true;

        /// <summary>达到最大反弹次数后是否禁用碰撞</summary>
        public bool DisableCollisionOnMaxBounces { get; set; } = false;

        /// <summary>反弹后是否触发 OnKill 效果（如爆炸）</summary>
        public bool TriggerKillOnMaxBounces { get; set; } = false;

        /// <summary>速度过低时是否停止弹幕</summary>
        public bool StopOnLowSpeed { get; set; } = true;

        /// <summary>停止阈值</summary>
        public float LowSpeedThreshold { get; set; } = 1f;

        /// <summary>停止后剩余时间</summary>
        public int TimeLeftAfterStop { get; set; } = 30;

        /// <summary>反弹时调用的回调</summary>
        public System.Action<Projectile, Vector2> OnBounce { get; set; } = null;

        public BounceBehavior() { }

        public BounceBehavior(int maxBounces, float bounceFactor = 0.4f)
        {
            MaxBounces = maxBounces;
            BounceFactor = bounceFactor;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            BounceCount = 0;
        }

        public void Update(Projectile projectile) { }

        /// <summary>
        /// 处理碰撞反弹。在弹幕的 OnTileCollide 中调用此方法。
        /// 返回 true 表示弹幕应销毁，false 表示弹幕继续存在。
        ///
        /// Terraria 引擎机制：OnTileCollide 返回 false 时，
        /// 引擎不会自动修改速度，仅保留弹幕存活。
        /// 因此这里直接设置反弹后的速度即可。
        /// </summary>
        public bool HandleTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            BounceCount++;

            if (BounceCount > MaxBounces)
            {
                if (KillOnMaxBounces)
                {
                    if (TriggerKillOnMaxBounces)
                    {
                        // 让引擎调用 OnKill
                        return true;
                    }
                    projectile.tileCollide = false;
                    if (DisableCollisionOnMaxBounces)
                    {
                        return false;
                    }
                    return true;
                }
                if (DisableCollisionOnMaxBounces)
                {
                    projectile.tileCollide = false;
                    return false;
                }
                return true;
            }

            // 直接设置反弹速度（与星火弹原始实现一致）
            if (oldVelocity.X != projectile.velocity.X)
            {
                projectile.velocity.X = -oldVelocity.X * BounceFactor;
            }
            if (oldVelocity.Y != projectile.velocity.Y)
            {
                projectile.velocity.Y = -oldVelocity.Y * BounceFactor;
            }

            // 速度过低时停止
            if (StopOnLowSpeed &&
                System.Math.Abs(projectile.velocity.X) < LowSpeedThreshold &&
                System.Math.Abs(projectile.velocity.Y) < LowSpeedThreshold)
            {
                projectile.velocity *= 0f;
                if (projectile.timeLeft > TimeLeftAfterStop)
                    projectile.timeLeft = TimeLeftAfterStop;
            }

            // 回调
            OnBounce?.Invoke(projectile, oldVelocity);

            return false; // 不销毁
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            BounceCount = 0;
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        /// <summary>
        /// IBulletBehavior.OnTileCollide 实现 — 委托给 HandleTileCollide。
        /// 返回 true 表示弹幕应销毁，false 表示继续存在，null 表示不处理。
        /// </summary>
        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            return HandleTileCollide(projectile, oldVelocity);
        }
    }
}
