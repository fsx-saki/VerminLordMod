using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 召唤物行为接口 — 所有召唤物行为必须实现此接口。
    ///
    /// 与 IBulletBehavior 的关键区别：
    /// - Update 接收 Player owner 参数，因为召唤物需要感知玩家位置/状态
    /// - 召唤物是持久存在的，行为需要处理状态切换（待机/跟随/攻击/返回）
    ///
    /// 设计哲学：
    /// 每个行为是一个独立的"能力模块"，通过组合模式叠加到召唤物上。
    /// 例如：OrbitMovement + RangedAttack + BobVisual = 环绕射击型召唤物
    ///       ChargeMovement + MeleeAttack + TrailVisual = 冲锋近战型召唤物
    /// </summary>
    public interface ISummonBehavior
    {
        /// <summary>行为名称（调试用）</summary>
        string Name { get; }

        /// <summary>
        /// 召唤物生成时调用。
        /// 用于初始化内部状态、纹理等。
        /// </summary>
        void OnSpawn(Projectile projectile, Player owner, IEntitySource source);

        /// <summary>
        /// 每帧更新。
        /// 这是行为的主要逻辑入口：移动、攻击、视觉效果等。
        /// </summary>
        void Update(Projectile projectile, Player owner);

        /// <summary>
        /// 命中 NPC 时调用。
        /// 用于施加 debuff、触发特效等。
        /// </summary>
        void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone);

        /// <summary>
        /// 召唤物销毁时调用。
        /// 用于清理资源、生成销毁粒子等。
        /// </summary>
        void OnKill(Projectile projectile, int timeLeft);

        /// <summary>
        /// 自定义绘制。
        /// 返回 true 表示引擎继续默认绘制，false 表示完全自定义绘制。
        /// </summary>
        bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch);

        /// <summary>
        /// 碰撞物块时调用。
        /// 返回 true=销毁, false=继续存在, null=不处理。
        /// </summary>
        bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity);
    }
}