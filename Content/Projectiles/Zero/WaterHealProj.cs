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

            // 4. 周期性治愈粒子 — 飞行时散发柔和光点
            Behaviors.Add(new PeriodicDustBehavior
            {
                SpawnChance = 0.5f,
                DustType = DustID.HealingPlus,
                Color = new Color(100, 255, 150, 150),
                ScaleMin = 0.5f,
                ScaleMax = 1.0f,
                Speed = 0.5f,
                SpreadRadius = 8f,
                NoGravity = true,
            });

            // 5. 环境光 — 柔和绿光
            Behaviors.Add(new GlowLightBehavior(new Vector3(0.2f, 0.6f, 0.3f)));

            // 6. 命中治疗 — 按伤害比例回复玩家生命
            Behaviors.Add(new HealOnHitBehavior(HealMultiplier)
            {
                HealDustCount = 8,
                DustType = DustID.HealingPlus,
                DustColor = new Color(100, 255, 150, 200),
                DustScaleMin = 0.8f,
                DustScaleMax = 1.5f,
                DustSpeed = 3f,
                DustSpreadRadius = 15f,
            });

            // 7. 销毁粉尘 — 消散时产生治愈水花
            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new System.Collections.Generic.List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 6,
                        DustType = DustID.HealingPlus,
                        Color = new Color(100, 255, 150, 180),
                        ScaleMin = 0.5f,
                        ScaleMax = 1.0f,
                        NoGravity = true,
                        SpreadRadius = 8f,
                        UseCircularVelocity = true,
                        SpeedMin = 0f,
                        SpeedMax = 3f,
                    },
                }
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
    }
}
