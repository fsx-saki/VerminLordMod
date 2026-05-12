using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 友方追踪行为 — 弹幕向最近的友方单位（自身/同队玩家）平滑转向。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new FriendlyHomingBehavior
    /// {
    ///     HomingStrength = 0.08f,
    ///     MaxSpeed = 6f,
    ///     DetectionRange = 400f,
    /// });
    /// </code>
    /// </summary>
    public class FriendlyHomingBehavior : IBulletBehavior
    {
        public string Name => "FriendlyHoming";

        /// <summary>追踪强度（0~1），越大转向越快</summary>
        public float HomingStrength { get; set; } = 0.08f;

        /// <summary>最大飞行速度</summary>
        public float MaxSpeed { get; set; } = 6f;

        /// <summary>检测友方范围（像素）</summary>
        public float DetectionRange { get; set; } = 400f;

        /// <summary>到达目标的最小距离（小于此距离停止追踪）</summary>
        public float ArriveDistance { get; set; } = 20f;

        /// <summary>是否自动旋转贴图</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        public FriendlyHomingBehavior() { }

        public FriendlyHomingBehavior(float homingStrength, float maxSpeed, float detectionRange = 400f)
        {
            HomingStrength = homingStrength;
            MaxSpeed = maxSpeed;
            DetectionRange = detectionRange;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            Vector2 target = FindNearestFriendly(projectile);
            if (target == Vector2.Zero) return;

            Vector2 toTarget = target - projectile.Center;
            float dist = toTarget.Length();
            if (dist < ArriveDistance) return;

            Vector2 desiredVel = toTarget.SafeNormalize(Vector2.Zero) * MaxSpeed;
            projectile.velocity = Vector2.Lerp(projectile.velocity, desiredVel, HomingStrength);

            if (projectile.velocity.Length() > MaxSpeed)
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * MaxSpeed;

            if (AutoRotate)
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;

        private Vector2 FindNearestFriendly(Projectile projectile)
        {
            Vector2 best = Vector2.Zero;
            float bestDist = DetectionRange;

            Player owner = Main.player[projectile.owner];

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player == null || !player.active || player.dead) continue;

                if (i != projectile.owner && owner != null && owner.active && player.team != owner.team)
                    continue;

                float dist = Vector2.Distance(projectile.Center, player.Center);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = player.Center;
                }
            }

            return best;
        }
    }
}