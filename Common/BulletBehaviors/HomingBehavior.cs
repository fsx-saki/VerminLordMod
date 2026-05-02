using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 追踪行为 — 弹幕向最近的敌人追踪飞行。
    /// 使用指数平滑插值，模拟 CycloneProj 的追踪风格。
    /// </summary>
    public class HomingBehavior : IBulletBehavior
    {
        public string Name => "Homing";

        /// <summary>追踪速度（像素/帧）</summary>
        public float Speed { get; set; } = 10f;

        /// <summary>追踪权重（0~1），越高追踪越灵敏</summary>
        public float TrackingWeight { get; set; } = 1f / 11f;

        /// <summary>追踪范围（像素），0 表示无限</summary>
        public float Range { get; set; } = 0f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        /// <summary>是否只追踪玩家（false=追踪任何可攻击的 NPC）</summary>
        public bool TrackPlayer { get; set; } = false;

        /// <summary>追踪目标类型（null=自动选择最近敌人）</summary>
        public NPC LockedTarget { get; set; } = null;

        /// <summary>是否启用光照</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public HomingBehavior() { }

        public HomingBehavior(float speed, float trackingWeight = 1f / 11f)
        {
            Speed = speed;
            TrackingWeight = trackingWeight;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            NPC target = LockedTarget;

            // 自动选择最近的敌人
            if (target == null || !target.active)
            {
                target = FindNearestEnemy(projectile.Center, Range);
            }

            if (target != null && target.active)
            {
                Vector2 targetPos = target.Center;
                Vector2 desiredVel = Vector2.Normalize(targetPos - projectile.Center) * Speed;
                projectile.velocity = (desiredVel + projectile.velocity * (1f / TrackingWeight - 1f)) * TrackingWeight;
            }
            else if (Speed > 0f && projectile.velocity != Vector2.Zero)
            {
                // 无目标时保持当前速度
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

        private NPC FindNearestEnemy(Vector2 center, float range)
        {
            NPC nearest = null;
            float nearestDist = range > 0f ? range : float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }

            return nearest;
        }
    }
}
