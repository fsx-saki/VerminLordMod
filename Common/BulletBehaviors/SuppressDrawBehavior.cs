using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 抑制绘制行为 — 阻止引擎默认绘制弹幕贴图。
    /// 适用于只需要粒子/拖尾效果、不需要显示弹幕本体的弹幕。
    /// 
    /// 使用方式：
    ///   Behaviors.Add(new SuppressDrawBehavior());
    ///   
    /// 注意：此行为必须在行为列表的最后添加，或者在 PreDraw 中
    /// 最后一个返回 false 的行为会覆盖前面的 true。
    /// 查看 BaseBullet.PreDraw 的实现确认行为顺序的影响。
    /// </summary>
    public class SuppressDrawBehavior : IBulletBehavior
    {
        public string Name => "SuppressDraw";

        public SuppressDrawBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        /// <summary>
        /// 返回 false 阻止引擎默认绘制。
        /// </summary>
        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return false;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
