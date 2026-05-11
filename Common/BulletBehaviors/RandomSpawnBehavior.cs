using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 随机散布生成行为 — 弹幕生成时在指定半径内随机偏移初始位置和速度方向。
    ///
    /// 适用于"散射/霰弹/随机散布"类攻击方式，例如：
    /// - 水弹在鼠标位置周围随机散布
    /// - 任何需要"不精确瞄准"的弹幕
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new RandomSpawnBehavior
    /// {
    ///     SpreadRadius = 40f,       // 位置随机偏移半径
    ///     AngleSpread = 0.3f,       // 速度方向随机偏移（弧度）
    ///     SpeedVariation = 0.2f,    // 速度随机变化比例（±20%）
    /// });
    /// </code>
    ///
    /// 注意：此行为只影响生成时的初始位置和速度，不影响后续飞行。
    /// 后续飞行由其他行为（如 AimBehavior、GravityBehavior）控制。
    /// </summary>
    public class RandomSpawnBehavior : IBulletBehavior
    {
        public string Name => "RandomSpawn";

        /// <summary>位置随机偏移半径（像素），默认 30</summary>
        public float SpreadRadius { get; set; } = 30f;

        /// <summary>速度方向随机偏移（弧度），默认 0.15</summary>
        public float AngleSpread { get; set; } = 0.15f;

        /// <summary>速度大小随机变化比例（0~1），默认 0.1（±10%）</summary>
        public float SpeedVariation { get; set; } = 0.1f;

        /// <summary>是否保留武器赋予的初始速度方向作为基准（true=在武器方向上偏移，false=完全随机）</summary>
        public bool UseBaseDirection { get; set; } = true;

        /// <summary>是否在生成时应用随机偏移（如果为 false，可在后续手动触发）</summary>
        public bool ApplyOnSpawn { get; set; } = true;

        public RandomSpawnBehavior() { }

        public RandomSpawnBehavior(float spreadRadius, float angleSpread = 0.15f, float speedVariation = 0.1f)
        {
            SpreadRadius = spreadRadius;
            AngleSpread = angleSpread;
            SpeedVariation = speedVariation;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!ApplyOnSpawn) return;

            // 1. 位置随机偏移
            if (SpreadRadius > 0f)
            {
                projectile.Center += Main.rand.NextVector2Circular(SpreadRadius, SpreadRadius);
                projectile.position = projectile.Center - new Vector2(projectile.width * 0.5f, projectile.height * 0.5f);
            }

            // 2. 速度方向随机偏移
            if (AngleSpread > 0f && projectile.velocity != Vector2.Zero)
            {
                float baseAngle = projectile.velocity.ToRotation();
                float randomAngle = baseAngle + Main.rand.NextFloat(-AngleSpread, AngleSpread);
                float speed = projectile.velocity.Length();

                // 速度大小随机变化
                if (SpeedVariation > 0f)
                {
                    speed *= 1f + Main.rand.NextFloat(-SpeedVariation, SpeedVariation);
                }

                projectile.velocity = randomAngle.ToRotationVector2() * speed;
            }
            else if (SpeedVariation > 0f && projectile.velocity != Vector2.Zero)
            {
                // 仅变化速度大小
                float speed = projectile.velocity.Length();
                speed *= 1f + Main.rand.NextFloat(-SpeedVariation, SpeedVariation);
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
            }
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
