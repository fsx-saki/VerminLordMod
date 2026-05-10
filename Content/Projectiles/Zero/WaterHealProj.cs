using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 水道治疗弹 — 治疗水弹。
    /// 水道技术储备库的"治疗/回复"技术：
    /// - 弹幕飞向鼠标位置（或队友），命中后回复生命
    /// - 对友方 NPC 也有效
    /// - 产生治愈水花粒子效果
    /// - 碰到物块时消散
    ///
    /// 行为组合：
    /// - AimBehavior: 沿鼠标方向飞行
    /// - WaterTrailBehavior: 水系拖尾（治愈泡泡）
    /// - LiquidBurstBehavior: 命中时水花爆裂
    /// - 自定义 OnHit: 回复生命
    /// </summary>
    public class WaterHealProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 8f;

        /// <summary>治疗量倍率（基于伤害值）</summary>
        private const float HealMultiplier = 0.5f;

        protected override void RegisterBehaviors()
        {
            // 1. 沿鼠标方向飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 水系拖尾 — 治愈水滴（柔和、缓慢）
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 60,
                ParticleLife = 40,
                SizeMultiplier = 0.5f,
                SplashSpeed = 2f,
                SplashAngle = 1.0f,
                InertiaFactor = 0.5f,
                Gravity = 0.04f,
                AirResistance = 0.96f,
                BubbleChance = 0.1f,
                BubbleSizeMultiplier = 1.5f,
                ColorStart = new Color(30, 180, 80, 200),    // 深治愈绿
                ColorEnd = new Color(50, 200, 100, 0),
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 3. 命中时水花爆裂（治愈水花）
            Behaviors.Add(new LiquidBurstBehavior(10, 4f)
            {
                ColorStart = new Color(100, 255, 150, 200),
                ColorEnd = new Color(50, 200, 100, 0),
                SizeMultiplier = 0.6f,
                NoGravity = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 治愈粒子效果（柔和光点）
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.HealingPlus,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0,
                    new Color(100, 255, 150, 150),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }

            // 柔和绿光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.6f, 0.3f);
        }

        /// <summary>
        /// 命中 NPC 时回复玩家生命
        /// </summary>
        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 回复玩家生命
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                int healAmount = (int)(damageDone * HealMultiplier);
                if (healAmount > 0)
                {
                    owner.statLife += healAmount;
                    owner.HealEffect(healAmount);

                    // 产生治愈光效
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                        Dust d = Dust.NewDustPerfect(
                            owner.Center + Main.rand.NextVector2Circular(15f, 15f),
                            DustID.HealingPlus,
                            vel,
                            0,
                            new Color(100, 255, 150, 200),
                            Main.rand.NextFloat(0.8f, 1.5f)
                        );
                        d.noGravity = true;
                    }
                }
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消散时产生治愈水花
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.HealingPlus,
                    vel,
                    0,
                    new Color(100, 255, 150, 180),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
