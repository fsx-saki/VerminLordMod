using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 吸附行为 — 将范围内的敌人拉向弹幕中心。
    ///
    /// 适用于漩涡、黑洞、龙卷风等吸附类弹幕。
    /// 支持配置吸附范围、强度、切向旋转效果、速度上限等。
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new PullBehavior
    /// {
    ///     PullRange = 200f,          // 吸附范围（像素）
    ///     PullStrength = 0.25f,      // 吸附力强度
    ///     TangentFactor = 0.5f,      // 切向旋转因子（0=纯径向，越大旋转越强）
    ///     MaxPullSpeed = 12f,        // 最大吸附速度
    ///     StrengthCurve = (dist, range) => 1f - dist / range,  // 自定义力度曲线
    /// });
    /// </code>
    /// </summary>
    public class PullBehavior : IBulletBehavior
    {
        public string Name => "Pull";

        /// <summary>吸附范围（像素），默认 160</summary>
        public float PullRange { get; set; } = 160f;

        /// <summary>吸附力强度，默认 0.15</summary>
        public float PullStrength { get; set; } = 0.15f;

        /// <summary>
        /// 切向旋转因子（0~1）。
        /// 0 = 纯径向吸附（直线拉向中心）
        /// 1 = 最大切向旋转（绕中心旋转）
        /// 默认 0.5
        /// </summary>
        public float TangentFactor { get; set; } = 0.5f;

        /// <summary>NPC 最大吸附速度，默认 8</summary>
        public float MaxPullSpeed { get; set; } = 8f;

        /// <summary>
        /// 自定义力度曲线函数。
        /// 参数：(distance, range) → 返回 0~1 的力度系数。
        /// 默认：线性衰减 (1 - dist/range)
        /// </summary>
        public System.Func<float, float, float> StrengthCurve { get; set; } = null;

        /// <summary>是否吸附友方 NPC（默认 false）</summary>
        public bool PullFriendly { get; set; } = false;

        /// <summary>是否吸附玩家（默认 false）</summary>
        public bool PullPlayers { get; set; } = false;

        /// <summary>位置偏移（相对于弹幕中心）</summary>
        public Vector2 PositionOffset { get; set; } = Vector2.Zero;

        /// <summary>是否启用光照效果</summary>
        public bool EnableLight { get; set; } = false;

        /// <summary>光照颜色</summary>
        public Vector3 LightColor { get; set; } = Vector3.Zero;

        public PullBehavior() { }

        public PullBehavior(float pullRange, float pullStrength, float tangentFactor = 0.5f)
        {
            PullRange = pullRange;
            PullStrength = pullStrength;
            TangentFactor = tangentFactor;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            Vector2 center = projectile.Center + PositionOffset;

            // 吸附 NPC
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly && !PullFriendly)
                    continue;
                if (!npc.CanBeChasedBy() && !PullFriendly)
                    continue;

                ApplyPull(npc, center);
            }

            // 吸附玩家
            if (PullPlayers)
            {
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.active && !player.dead)
                    {
                        ApplyPull(player, center);
                    }
                }
            }

            // 光照
            if (EnableLight && LightColor != Vector3.Zero)
            {
                Lighting.AddLight(center, LightColor.X, LightColor.Y, LightColor.Z);
            }
        }

        /// <summary>
        /// 对单个实体施加吸附力
        /// </summary>
        private void ApplyPull(Entity entity, Vector2 center)
        {
            Vector2 toCenter = center - entity.Center;
            float dist = toCenter.Length();

            if (dist <= PullRange && dist > 5f)
            {
                // 力度系数（默认线性衰减）
                float strengthFactor;
                if (StrengthCurve != null)
                {
                    strengthFactor = StrengthCurve(dist, PullRange);
                }
                else
                {
                    strengthFactor = 1f - dist / PullRange;
                }

                // 径向吸附力
                float pullForce = PullStrength * strengthFactor;
                Vector2 radialVelocity = toCenter.SafeNormalize(Vector2.Zero) * pullForce;

                // 切向旋转速度
                if (TangentFactor > 0f)
                {
                    Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X).SafeNormalize(Vector2.Zero);
                    float tangentForce = pullForce * TangentFactor;
                    radialVelocity += tangent * tangentForce;
                }

                entity.velocity += radialVelocity;

                // 限制最大速度
                if (entity.velocity.Length() > MaxPullSpeed)
                {
                    entity.velocity = entity.velocity.SafeNormalize(Vector2.Zero) * MaxPullSpeed;
                }
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
