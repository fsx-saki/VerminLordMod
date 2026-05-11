using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 发光行为 — 每帧在弹幕位置添加环境光。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new GlowLightBehavior
    /// {
    ///     LightColor = new Vector3(0.2f, 0.4f, 0.8f),
    /// });
    /// </code>
    /// </summary>
    public class GlowLightBehavior : IBulletBehavior
    {
        public string Name => "GlowLight";

        /// <summary>光照颜色 (R, G, B)，范围 0~1</summary>
        public Vector3 LightColor { get; set; } = Vector3.One;

        public GlowLightBehavior() { }

        public GlowLightBehavior(Vector3 lightColor)
        {
            LightColor = lightColor;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            if (LightColor != Vector3.Zero)
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}