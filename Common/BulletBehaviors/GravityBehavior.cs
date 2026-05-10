using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 重力行为 — 弹幕每帧受重力影响。
    /// 可配置重力加速度、最大下落速度。
    /// </summary>
    public class GravityBehavior : IBulletBehavior
    {
        public string Name => "Gravity";

        /// <summary>重力加速度（像素/帧²）</summary>
        public float Acceleration { get; set; } = 0.3f;

        /// <summary>最大下落速度（像素/帧）</summary>
        public float MaxFallSpeed { get; set; } = 12f;

        /// <summary>重力方向（默认向下）</summary>
        public Vector2 GravityDirection { get; set; } = new Vector2(0f, 1f);

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = false;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        public GravityBehavior() { }

        public GravityBehavior(float acceleration, float maxFallSpeed = 12f)
        {
            Acceleration = acceleration;
            MaxFallSpeed = maxFallSpeed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            // 应用重力
            projectile.velocity += GravityDirection * Acceleration;

            // 限制最大下落速度（沿重力方向的分量）
            float velAlongGravity = Vector2.Dot(projectile.velocity, GravityDirection);
            if (velAlongGravity > MaxFallSpeed)
            {
                Vector2 gravityComponent = GravityDirection * (velAlongGravity - MaxFallSpeed);
                projectile.velocity -= gravityComponent;
            }

            // 自动旋转
            if (AutoRotate && projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
