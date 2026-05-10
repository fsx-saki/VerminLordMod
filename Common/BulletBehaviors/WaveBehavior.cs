using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 波浪飞行行为 — 弹幕沿正弦波轨迹飞行。
    /// 适用于 ShaZhaoJiuYueZhan 等有弯曲轨迹的弹幕。
    /// </summary>
    public class WaveBehavior : IBulletBehavior
    {
        public string Name => "Wave";

        /// <summary>波浪幅度（弧度）</summary>
        public float Amplitude { get; set; } = 0.02f;

        /// <summary>波浪频率</summary>
        public float Frequency { get; set; } = 0.05f;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = MathHelper.PiOver2;

        public WaveBehavior() { }

        public WaveBehavior(float amplitude, float frequency)
        {
            Amplitude = amplitude;
            Frequency = frequency;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile)
        {
            // 对速度施加正弦旋转
            float waveAngle = (float)System.Math.Sin(Main.GameUpdateCount * Frequency) * Amplitude;
            projectile.velocity = projectile.velocity.RotatedBy(waveAngle);

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
