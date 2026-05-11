using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 约束转向行为 — 加速度方向被限制在速度垂线两侧各 ±30° 的锥形范围内，
    /// 加速度大小恒定。弹幕只能通过"转向"来追踪目标，不能直接加减速。
    ///
    /// 设计哲学：
    /// 模拟真实物理中的"向心加速度"——力始终垂直于速度方向（或接近垂直），
    /// 只改变方向不改变速率。这产生平滑的圆弧追踪轨迹，而非生硬的直线追踪。
    ///
    /// 加速度约束：
    /// - 允许方向：速度左垂线 ± ConeHalfAngle ∪ 速度右垂线 ± ConeHalfAngle
    /// - 即加速度与速度的夹角必须在 [90°-ConeHalfAngle, 90°+ConeHalfAngle] 范围内
    /// - 默认 ConeHalfAngle = 30°（π/6），即加速度在速度的 [60°, 120°] 范围内
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new ConstrainedSteerBehavior
    /// {
    ///     TrackMouse = true,
    ///     AccelMagnitude = 0.3f,
    ///     MaxSpeed = 12f,
    ///     ConeHalfAngle = MathHelper.Pi / 6f,  // 30°
    /// });
    /// </code>
    /// </summary>
    public class ConstrainedSteerBehavior : IBulletBehavior
    {
        public string Name => "ConstrainedSteer";

        /// <summary>是否追踪鼠标位置</summary>
        public bool TrackMouse { get; set; } = true;

        /// <summary>是否追踪最近敌人</summary>
        public bool TrackNPC { get; set; } = false;

        /// <summary>追踪范围（像素），0 表示无限</summary>
        public float Range { get; set; } = 0f;

        /// <summary>加速度大小（像素/帧²），恒定不变</summary>
        public float AccelMagnitude { get; set; } = 0.3f;

        /// <summary>最大速度（像素/帧），防止无限加速</summary>
        public float MaxSpeed { get; set; } = 12f;

        /// <summary>加速度锥形半角（弧度），默认 30°（π/6）</summary>
        public float ConeHalfAngle { get; set; } = MathHelper.Pi / 6f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public ConstrainedSteerBehavior() { }

        public ConstrainedSteerBehavior(float accelMagnitude, float maxSpeed)
        {
            AccelMagnitude = accelMagnitude;
            MaxSpeed = maxSpeed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            Vector2 targetPos = GetTargetPosition(projectile);
            Vector2 toTarget = targetPos - projectile.Center;

            if (toTarget == Vector2.Zero)
                return;

            float dist = toTarget.Length();
            if (Range > 0f && dist > Range)
                return;

            Vector2 targetDir = toTarget.SafeNormalize(Vector2.Zero);
            Vector2 velDir = projectile.velocity.SafeNormalize(Vector2.Zero);

            if (velDir == Vector2.Zero)
            {
                projectile.velocity = targetDir * AccelMagnitude;
                return;
            }

            float cross = velDir.X * targetDir.Y - velDir.Y * targetDir.X;

            Vector2 perp;
            if (cross > 0f)
                perp = new Vector2(-velDir.Y, velDir.X);
            else if (cross < 0f)
                perp = new Vector2(velDir.Y, -velDir.X);
            else
                return;

            float dot = Vector2.Dot(perp, targetDir);
            float angleFromPerp = (float)System.Math.Acos(MathHelper.Clamp(dot, -1f, 1f));

            float clampedAngle = angleFromPerp < ConeHalfAngle ? angleFromPerp : ConeHalfAngle;

            float perpCrossTarget = perp.X * targetDir.Y - perp.Y * targetDir.X;
            float angleSign = perpCrossTarget >= 0f ? 1f : -1f;

            Vector2 accelDir = perp.RotatedBy(clampedAngle * angleSign);
            projectile.velocity += accelDir * AccelMagnitude;

            float speed = projectile.velocity.Length();
            if (speed > MaxSpeed)
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;

            if (AutoRotate && projectile.velocity != Vector2.Zero)
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;

            if (EnableLight && LightColor != Vector3.Zero)
                Lighting.AddLight(projectile.Center, LightColor.X, LightColor.Y, LightColor.Z);
        }

        private Vector2 GetTargetPosition(Projectile projectile)
        {
            if (TrackMouse)
                return Main.MouseWorld;

            if (TrackNPC)
            {
                NPC target = FindNearestNPC(projectile.Center, Range);
                if (target != null)
                    return target.Center;
            }

            return projectile.Center;
        }

        private static NPC FindNearestNPC(Vector2 center, float maxRange)
        {
            NPC nearest = null;
            float nearestDist = maxRange > 0f ? maxRange : float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.DistanceSquared(center, npc.Center);
                if (dist < nearestDist * nearestDist)
                {
                    nearestDist = (float)System.Math.Sqrt(dist);
                    nearest = npc;
                }
            }

            return nearest;
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