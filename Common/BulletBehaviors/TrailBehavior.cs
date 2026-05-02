using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 拖尾行为 — 将 TrailManager 包装为 IBulletBehavior。
    /// 自动管理 TrailManager 的 Update/Draw 生命周期。
    /// </summary>
    public class TrailBehavior : IBulletBehavior
    {
        public string Name => "Trail";

        /// <summary>拖尾管理器实例</summary>
        public TrailManager TrailManager { get; } = new TrailManager();

        /// <summary>
        /// 是否在 PreDraw 中由本行为绘制拖尾。
        /// 如果为 false，需要外部自行调用 TrailManager.Draw()。
        /// </summary>
        public bool AutoDraw { get; set; } = true;

        /// <summary>
        /// 是否在 PreDraw 中返回 false（阻止引擎默认绘制）。
        /// 当弹幕有自定义绘制（如发光层）时需要设为 true。
        /// </summary>
        public bool SuppressDefaultDraw { get; set; } = false;

        public TrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            TrailManager.Update(projectile.Center, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            TrailManager.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (AutoDraw)
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }
    }
}
