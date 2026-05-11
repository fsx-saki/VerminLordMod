using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 召唤物基类 — 所有使用行为系统的召唤物应继承此类。
    ///
    /// 通过 sealed 关键字锁定生命周期方法，确保行为系统正确执行。
    /// 子类只需实现 RegisterBehaviors() 来组合行为模块。
    ///
    /// 与 BaseBullet 的对应关系：
    /// - BaseBullet 用于一次性弹幕（发射→飞行→命中→销毁）
    /// - SummonMinion 用于持久召唤物（生成→跟随→攻击→跟随→...）
    ///
    /// 内置功能：
    /// - 自动管理 Buff（CheckActive）
    /// - 提供 Owner 引用
    /// - 行为生命周期委托
    /// - 默认 Minion 属性设置
    /// </summary>
    public abstract class SummonMinion : ModProjectile
    {
        /// <summary>行为列表（组合模式的核心）</summary>
        protected List<ISummonBehavior> Behaviors { get; } = new List<ISummonBehavior>();

        /// <summary>
        /// 子类在此方法中注册行为。
        /// 例如：Behaviors.Add(new OrbitMovement(orbitDistance: 100f));
        /// </summary>
        protected abstract void RegisterBehaviors();

        /// <summary>
        /// 返回此召唤物对应的 Buff 类型。
        /// 用于 CheckActive 自动管理。
        /// </summary>
        protected abstract int SummonBuffType { get; }

        public sealed override void OnSpawn(IEntitySource source)
        {
            Player owner = Main.player[Projectile.owner];
            RegisterBehaviors();
            foreach (var behavior in Behaviors)
            {
                behavior.OnSpawn(Projectile, owner, source);
            }
            OnSpawned(owner, source);
        }

        public sealed override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            foreach (var behavior in Behaviors)
            {
                behavior.Update(Projectile, owner);
            }

            OnAI(owner);
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
                if (!behavior.PreDraw(Projectile, ref lightColor, Main.spriteBatch))
                    drawDefault = false;
            }
            return drawDefault;
        }

        public sealed override bool OnTileCollide(Vector2 oldVelocity)
        {
            bool shouldKill = false;
            foreach (var behavior in Behaviors)
            {
                bool? result = behavior.OnTileCollide(Projectile, oldVelocity);
                if (result == true)
                    shouldKill = true;
            }
            if (shouldKill)
                return true;

            return OnTileCollided(oldVelocity);
        }

        /// <summary>
        /// 检查召唤物是否应保持活跃。
        /// 自动处理玩家死亡/离线的 Buff 清理。
        /// </summary>
        protected virtual bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(SummonBuffType);
                return false;
            }

            if (owner.HasBuff(SummonBuffType))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        // ===== 可选扩展点 =====

        /// <summary>在 RegisterBehaviors 和 behaviors.OnSpawn 之后调用</summary>
        protected virtual void OnSpawned(Player owner, IEntitySource source) { }

        /// <summary>在 behaviors.Update 之后调用</summary>
        protected virtual void OnAI(Player owner) { }

        /// <summary>在 behaviors.OnHitNPC 之后调用</summary>
        protected virtual void OnHit(NPC target, NPC.HitInfo hit, int damageDone) { }

        /// <summary>在 behaviors.OnKill 之后调用</summary>
        protected virtual void OnKilled(int timeLeft) { }

        /// <summary>
        /// 在 behaviors.OnTileCollide 之后调用。
        /// 返回 true 表示弹幕应销毁，false 表示继续存在。
        /// 默认返回 false（不销毁，召唤物通常不因碰墙销毁）。
        /// </summary>
        protected virtual bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}