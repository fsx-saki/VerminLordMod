using System;
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
    /// 水道旋转水刃 — 高速旋转的月牙形水刃，斩击路径上的敌人。
    ///
    /// 设计哲学：
    /// 高压水流可以形成锋利的切割刃。旋转的水刃在视觉上更有"斩击感"，
    /// 配合贴图拖尾产生水花四溅的效果。
    ///
    /// 运动方式：
    /// - 直线飞向鼠标方向
    /// - 高速自转（旋转的水刃更有视觉冲击力）
    /// - 可穿透多个敌人
    ///
    /// 视觉效果：
    /// - 使用 LiquidTrailBehavior（与火焰同款贴图拖尾），水蓝渐变
    /// - 高速旋转 + 拖尾 = 旋转水花效果
    /// - 命中时沿法线泼洒 WaterDropProj
    /// - 发光层增强视觉
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - RotateBehavior: 高速自转
    /// - LiquidTrailBehavior: 贴图碎片拖尾
    /// - NormalBurstBehavior: 命中时沿法线泼洒水滴
    /// - GlowDrawBehavior: 发光绘制
    /// </summary>
    public class WaterSlashProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 12f;

        /// <summary>自转速度（弧度/帧）</summary>
        private const float SpinSpeed = 0.3f;

        /// <summary>最大存活时间（帧）</summary>
        private const int MaxLife = 90;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = false,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.7f)
            });

            // 2. 高速自转（旋转的水刃）
            Behaviors.Add(new RotateBehavior
            {
                RotationSpeed = SpinSpeed,
                OverrideAutoRotate = true
            });

            // 3. 贴图碎片拖尾（水蓝渐变 + 浮力，与火焰同款技术）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 40,
                FragmentLife = 18,
                SizeMultiplier = 0.6f,
                SpawnInterval = 1,
                AdaptiveDensity = true,
                AdaptiveSpeedThreshold = 4f,
                AdaptiveDensityFactor = 5f,
                AdaptiveLife = true,
                AdaptiveTargetLength = 70f,
                SpeedLifeExponent = 0.35f,
                MinFragmentLife = 4,
                ColorStart = new Color(50, 150, 255, 240),
                ColorEnd = new Color(20, 60, 180, 0),
                Buoyancy = 0.03f,
                AirResistance = 0.97f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.2f,
                SplashAngle = 0.5f,
                RandomSpread = 0.7f,
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 4. 发光绘制
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 200, 255, 100),
                GlowBaseScale = 1.2f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.25f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.3f, 0.7f)
            });

            // 5. 法线崩解（命中时沿法线泼洒水滴）
            Behaviors.Add(new NormalBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 10,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 5f,
                SpreadAngle = 0.5f,
                SideAngle = 1.0f,
                BackSplashChance = 0.02f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(50, 150, 255, 220),
                DustColorEnd = new Color(20, 60, 180, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = false,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
        }

        protected override void OnAI()
        {
            // 旋转时产生侧向水雾
            if (Main.rand.NextBool(2))
            {
                float rot = Projectile.rotation;
                Vector2 sideDir = new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot));
                Vector2 pos = Projectile.Center + sideDir * Main.rand.NextFloat(-12f, 12f);

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.MagicMirror,
                    sideDir * Main.rand.NextFloat(0.5f, 2f),
                    0,
                    new Color(80, 200, 255, 150),
                    Main.rand.NextFloat(0.3f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 额外水花
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.MagicMirror,
                    vel,
                    0,
                    new Color(80, 200, 255, 150),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        /// <summary>
        /// 碰到物块时销毁。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}