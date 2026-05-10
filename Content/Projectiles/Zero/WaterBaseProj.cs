using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Projectiles.Zero;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 水道基础弹幕 — 水弹
    /// 发射后受重力影响做抛物线运动，碰到障碍物就像水球一样破掉。
    ///
    /// 行为组合：
    /// - GravityBehavior: 重力影响，抛物线弹道
    /// - ParticleBodyBehavior: 由翻涌小水珠组成的水球本体
    /// - WaterTrailBehavior: 水系拖尾（水球表面脱落的水珠）
    /// - LiquidBurstBehavior: 命中/销毁时水花泼洒（像水球破掉摊开）
    /// - SuppressDrawBehavior: 阻止默认贴图绘制（本体由粒子组成）
    /// - OnTileCollided: 碰到物块即销毁（水球破掉）
    /// </summary>
    public class WaterBaseProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 重力影响 — 抛物线弹道（慢速，轻重力，模拟水球漂浮感）
            Behaviors.Add(new GravityBehavior(acceleration: 0.12f, maxFallSpeed: 6f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 粒子体 — 由翻涌小水珠组成的水球本体
            var bodyBehavior = new ParticleBodyBehavior(particleCount: 35, bodyRadius: 22f)
            {
                ParticleSize = 1.3f,
                ColorStart = new Color(20, 80, 200, 220),
                ColorEnd = new Color(20, 80, 200, 220),
                SwirlSpeed = 0.1f,
                ReturnForce = 0.8f,
                JitterStrength = 0.7f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.7f,
                EnableLight = false,
            };
            Behaviors.Add(bodyBehavior);

            // 3. 水系拖尾 — 水球表面脱落的水滴和泡泡（短粗，不向后飞溅）
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 40,
                ParticleLife = 14,
                SizeMultiplier = 0.6f,
                SpawnChance = 0.7f,
                SplashSpeed = 0.3f,
                SplashAngle = 0.2f,
                InertiaFactor = 0.03f,
                RandomSpread = 0.8f,
                Gravity = 0.2f,
                AirResistance = 0.98f,
                BubbleChance = 0.6f,
                BubbleSizeMultiplier = 2.0f,
                ColorStart = new Color(20, 80, 200, 220),
                ColorEnd = new Color(20, 80, 200, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 4. 粒子体崩解（命中/销毁时）— 生成 WaterDropProj 向四面八方飞散
            //    使用 ExplosionSpawnHelper 模式，与 BloodHandprintsProj → BloodDropProj 一致
            Behaviors.Add(new ParticleBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 12,
                SpeedMin = 3f,
                SpeedMax = 6f,
                SpreadRadius = 5f,
                AngleSpread = 0.4f,
                SpawnExtraDust = true,
                ExtraDustCount = 8,
                DustType = DustID.MagicMirror,
                DustColorStart = new Color(30, 100, 200, 200),
                DustColorEnd = new Color(30, 100, 200, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = false,
            });

            // 5. 阻止默认贴图绘制 — 本体由 ParticleBodyBehavior 的粒子组成
            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        /// <summary>
        /// 碰到物块时销毁弹幕（水球破掉）。
        /// LiquidBurstBehavior.OnKill 会自动触发水花泼洒效果。
        /// </summary>
        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
