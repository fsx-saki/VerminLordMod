using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 弹幕行为基类 — 所有使用行为系统的弹幕应继承此类。
    /// 通过 sealed 关键字锁定生命周期方法，确保行为系统正确执行。
    /// </summary>
    public abstract class BaseBullet : ModProjectile
    {
        /// <summary>行为列表（组合模式的核心）</summary>
        protected List<IBulletBehavior> Behaviors { get; } = new List<IBulletBehavior>();

        /// <summary>
        /// 子类在此方法中注册行为。
        /// 例如：Behaviors.Add(new AimBehavior(speed: 10f));
        /// </summary>
        protected abstract void RegisterBehaviors();

        public sealed override void OnSpawn(IEntitySource source)
        {
            RegisterBehaviors();
            foreach (var behavior in Behaviors)
            {
                behavior.OnSpawn(Projectile, source);
            }
            OnSpawned(source);
        }

        public sealed override void AI()
        {
            foreach (var behavior in Behaviors)
            {
                behavior.Update(Projectile);
            }
            OnAI();
        }

        public sealed override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            foreach (var behavior in Behaviors)
            {
                behavior.OnHitNPC(Projectile, target, hit, damageDone);
            }
            OnHit(target, hit, damageDone);
        }

        public sealed override void OnKill(int timeLeft)
        {
            foreach (var behavior in Behaviors)
            {
                behavior.OnKill(Projectile, timeLeft);
            }
            OnKilled(timeLeft);
        }

        public sealed override bool PreDraw(ref Color lightColor)
        {
            bool drawDefault = true;
            foreach (var behavior in Behaviors)
            {
                // 如果任一行为返回 false，则阻止默认绘制
                if (!behavior.PreDraw(Projectile, ref lightColor, Main.spriteBatch))
                    drawDefault = false;
            }
            return drawDefault;
        }

        // ===== 可选扩展点 =====

        /// <summary>在 RegisterBehaviors 和 behaviors.OnSpawn 之后调用</summary>
        protected virtual void OnSpawned(IEntitySource source) { }

        /// <summary>在 behaviors.Update 之后调用</summary>
        protected virtual void OnAI() { }

        /// <summary>在 behaviors.OnHitNPC 之后调用</summary>
        protected virtual void OnHit(NPC target, NPC.HitInfo hit, int damageDone) { }

        /// <summary>在 behaviors.OnKill 之后调用</summary>
        protected virtual void OnKilled(int timeLeft) { }
    }
}
