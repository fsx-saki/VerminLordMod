using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 自机狙行为 — 弹幕沿初始方向直线飞行。
    /// 覆盖了现有约 70% 弹幕的 AI 逻辑。
    /// </summary>
    public class AimBehavior : IBulletBehavior
    {
        public string Name => "Aim";

        /// <summary>飞行速度（像素/帧）</summary>
        public float Speed { get; set; }

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度），默认 +π/2 使弹幕尖端朝前</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>是否添加光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public AimBehavior(float speed)
        {
            Speed = speed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 设置初始速度
            if (Speed > 0f)
            {
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * Speed;
            }
        }

        public void Update(Projectile projectile)
        {
            // 保持速度恒定
            if (Speed > 0f && projectile.velocity != Vector2.Zero)
            {
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * Speed;
            }

            // 自动旋转
            if (AutoRotate && projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
            }

            // 光照
            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true; // 使用默认绘制
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
