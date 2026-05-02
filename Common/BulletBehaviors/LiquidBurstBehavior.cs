using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 液体爆裂行为 — 弹幕销毁时生成大量飞溅碎片，模拟水破裂泼洒效果。
    /// 
    /// 使用 Terraria 原生的 Dust 系统实现，不受 projectile 生命周期影响。
    /// 碎片使用自定义绘制（Dust + 自定义颜色/缩放），模拟 LiquidTrail 风格的飞溅效果。
    /// </summary>
    public class LiquidBurstBehavior : IBulletBehavior
    {
        public string Name => "LiquidBurst";

        // ===== 可配置参数 =====

        /// <summary>碎片数量</summary>
        public int FragmentCount { get; set; } = 20;

        /// <summary>碎片飞溅速度</summary>
        public float BurstSpeed { get; set; } = 6f;

        /// <summary>碎片大小倍率</summary>
        public float SizeMultiplier { get; set; } = 1f;

        /// <summary>起始颜色</summary>
        public Color ColorStart { get; set; } = new Color(80, 255, 50, 200);

        /// <summary>结束颜色</summary>
        public Color ColorEnd { get; set; } = new Color(20, 120, 10, 0);

        /// <summary>Dust 类型（默认 DustID.MagicMirror，支持自定义颜色）</summary>
        public int DustType { get; set; } = DustID.MagicMirror;

        /// <summary>是否无重力</summary>
        public bool NoGravity { get; set; } = true;

        /// <summary>是否在碎片上应用颜色渐变</summary>
        public bool UseColorLerp { get; set; } = true;

        // 内部状态
        private bool _hasBurst = false;

        public LiquidBurstBehavior() { }

        public LiquidBurstBehavior(int fragmentCount, float burstSpeed)
        {
            FragmentCount = fragmentCount;
            BurstSpeed = burstSpeed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasBurst = false;
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (_hasBurst) return;
            _hasBurst = true;

            Vector2 center = projectile.Center;

            for (int i = 0; i < FragmentCount; i++)
            {
                // 随机方向 + 速度
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(BurstSpeed * 0.5f, BurstSpeed * 1.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                // 随机缩放
                float scale = Main.rand.NextFloat(0.6f, 1.2f) * SizeMultiplier;

                // 随机颜色插值
                Color dustColor = ColorStart;
                if (UseColorLerp)
                {
                    float colorT = Main.rand.NextFloat(0f, 0.6f);
                    dustColor = Color.Lerp(ColorStart, ColorEnd, colorT);
                }

                // 使用 Dust.NewDust 生成粒子
                int dustId = Dust.NewDust(
                    projectile.position, projectile.width, projectile.height,
                    DustType,
                    vel.X, vel.Y,
                    100, dustColor, scale
                );

                // 配置 Dust 属性
                if (dustId >= 0 && dustId < Main.dust.Length)
                {
                    Dust d = Main.dust[dustId];
                    d.noGravity = NoGravity;
                    d.velocity = vel;
                    d.position = center - new Vector2(4f); // 居中
                    d.fadeIn = 0.5f; // 淡入效果
                }
            }

            // 额外生成一些更小的碎片（模拟水雾）
            for (int i = 0; i < FragmentCount / 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(BurstSpeed * 0.3f, BurstSpeed * 0.8f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                int dustId = Dust.NewDust(
                    projectile.position, projectile.width, projectile.height,
                    DustType,
                    vel.X, vel.Y,
                    80, Color.Lerp(ColorStart, ColorEnd, 0.7f),
                    Main.rand.NextFloat(0.3f, 0.6f) * SizeMultiplier
                );

                if (dustId >= 0 && dustId < Main.dust.Length)
                {
                    Dust d = Main.dust[dustId];
                    d.noGravity = true;
                    d.velocity = vel;
                    d.position = center - new Vector2(4f);
                    d.fadeIn = 0.3f;
                }
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            // Dust 由 Terraria 引擎自动绘制，无需额外处理
            return true;
        }
    }
}
