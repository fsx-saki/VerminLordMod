using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 弹幕行为接口 — 所有弹幕行为必须实现此接口。
    /// 通过组合模式，一个弹幕可以同时拥有多个行为（如 Aim + Trail + Glow）。
    /// </summary>
    public interface IBulletBehavior
    {
        /// <summary>行为名称（调试用）</summary>
        string Name { get; }

        /// <summary>
        /// 弹幕生成时调用（OnSpawn）。
        /// 用于初始化纹理、拖尾、状态等。
        /// </summary>
        void OnSpawn(Projectile projectile, IEntitySource source);

        /// <summary>
        /// 每帧更新（AI）。
        /// 用于控制弹幕移动、旋转、发光等。
        /// </summary>
        void Update(Projectile projectile);

        /// <summary>
        /// 命中 NPC 时调用（OnHitNPC）。
        /// </summary>
        void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone);

        /// <summary>
        /// 弹幕销毁时调用（OnKill）。
        /// 用于生成粒子、清除拖尾等。
        /// </summary>
        void OnKill(Projectile projectile, int timeLeft);

        /// <summary>
        /// 自定义绘制（PreDraw）。
        /// 返回 true 表示引擎继续默认绘制，false 表示完全自定义绘制。
        /// </summary>
        bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch);

        /// <summary>
        /// 碰撞物块时调用（OnTileCollide）。
        /// 返回 true 表示弹幕应销毁，false 表示弹幕继续存在，null 表示不处理（由其他行为或默认逻辑决定）。
        /// </summary>
        bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity);
    }
}
