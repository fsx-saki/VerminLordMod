using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 鼠标追踪行为 — 弹幕使用指数平滑插值追踪鼠标指针位置。
    ///
    /// 与 HomingBehavior（追踪敌人）不同，此行为追踪鼠标光标，
    /// 适合"引导弹"或"散射追踪"类攻击方式。
    ///
    /// 插值方式（指数平滑）：
    ///   v_new = (v_target + v_old × weight) / (weight + 1)
    ///
    /// weight 越大越平滑（反应越慢），weight 越小追踪越灵敏。
    ///
    /// 注意：OnSpawn 不会覆盖弹幕的初始速度，保留武器赋予的初速度。
    /// 这意味着如果弹幕以散射初速度生成，它会先沿散射方向飞行，
    /// 然后通过指数平滑逐渐转向鼠标，产生"导弹先散开再转大弯"的效果。
    /// </summary>
    public class MouseAimBehavior : IBulletBehavior
    {
        public string Name => "MouseAim";

        /// <summary>飞行速度（像素/帧）</summary>
        public float Speed { get; set; } = 8f;

        /// <summary>
        /// 平滑权重（越大越平滑）。
        /// weight=10 → α≈0.091（平滑拖尾感）
        /// weight=4  → α=0.2（反应更快）
        /// weight=19 → α=0.05（更平滑）
        /// </summary>
        public float SmoothWeight { get; set; } = 10f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public MouseAimBehavior() { }

        public MouseAimBehavior(float speed, float smoothWeight = 10f)
        {
            Speed = speed;
            SmoothWeight = smoothWeight;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 不覆盖初始速度，保留武器赋予的初速度（如散射方向的速度）
            // 这样弹幕会先沿初始方向飞行，再通过 Update 中的指数平滑逐渐转向鼠标
        }

        public void Update(Projectile projectile)
        {
            // 计算指向鼠标的目标速度
            Vector2 toMouse = Main.MouseWorld - projectile.Center;
            Vector2 targetVelocity = Vector2.Zero;

            if (toMouse != Vector2.Zero)
            {
                targetVelocity = toMouse.SafeNormalize(Vector2.Zero) * Speed;
            }

            // 指数平滑插值：v_new = (v_target + v_old * weight) / (weight + 1)
            // 初始时 v_old 是武器赋予的初速度，每帧向鼠标方向修正一点点，
            // 自然产生"先沿初始方向飞→逐渐转大弯→最终指向鼠标"的流畅轨迹
            if (targetVelocity != Vector2.Zero)
            {
                projectile.velocity = (targetVelocity + projectile.velocity * SmoothWeight) / (SmoothWeight + 1f);
            }
            else if (Speed > 0f && projectile.velocity != Vector2.Zero)
            {
                // 保底：保持当前速度
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
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
