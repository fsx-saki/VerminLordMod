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
    /// 水道圆环汇聚水弹 — 从鼠标位置周围大圆环上随机位置生成，向鼠标位置汇聚并穿过。
    ///
    /// 设计哲学：
    /// - 使用 RegionSpawnBehavior 在圆环上随机位置生成，初始速度指向鼠标
    /// - 使用 AimBehavior 直线飞向鼠标位置
    /// - 本体由粒子组成（ParticleBodyBehavior）
    /// - 到达鼠标位置后直接穿过继续飞行，不碎裂
    /// - 可穿透敌人，穿过时洒下 2-3 颗水滴
    /// - 碰到物块时销毁
    ///
    /// 行为组合：
    /// - RegionSpawnBehavior: 在圆环上随机位置生成，指向鼠标
    /// - AimBehavior: 直线飞行
    /// - ParticleBodyBehavior: 粒子水球本体
    /// - WaterTrailBehavior: 水系拖尾
    /// - SuppressDrawBehavior: 阻止默认贴图绘制
    /// </summary>
    public class WaterRingConvergeProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 10f;

        /// <summary>圆环内半径（像素）</summary>
        private const float RingInnerRadius = 80f;

        /// <summary>圆环外半径（像素）</summary>
        private const float RingOuterRadius = 150f;

        protected override void RegisterBehaviors()
        {
            // 1. 区域生成 — 在鼠标位置周围大圆环上随机位置生成，指向鼠标
            Behaviors.Add(new RegionSpawnBehavior
            {
                Shape = RegionSpawnBehavior.SpawnShape.Ring,
                InnerRadius = RingInnerRadius,
                OuterRadius = RingOuterRadius,
                TargetCenter = () => Main.MouseWorld,
                InitialSpeed = FlySpeed,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 2. 直线飞行（保持速度方向）
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 3. 粒子体 — 水球
            var bodyBehavior = new ParticleBodyBehavior(particleCount: 22, bodyRadius: 14f)
            {
                ParticleSize = 1.0f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 220),
                SwirlSpeed = 0.1f,
                ReturnForce = 0.8f,
                JitterStrength = 0.5f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.5f,
                EnableLight = false,
            };
            Behaviors.Add(bodyBehavior);

            // 4. 水系拖尾 — 水珠和泡泡
            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 28,
                ParticleLife = 10,
                SizeMultiplier = 0.45f,
                SpawnChance = 0.5f,
                SplashSpeed = 0.2f,
                SplashAngle = 0.15f,
                InertiaFactor = 0.03f,
                RandomSpread = 0.6f,
                Gravity = 0.15f,
                AirResistance = 0.97f,
                BubbleChance = 0.4f,
                BubbleSizeMultiplier = 1.5f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            // 5. 命中洒落 — 穿过敌人时洒下 2-3 颗水滴
            Behaviors.Add(new OnHitDropletBehavior(
                ModContent.ProjectileType<WaterDropProj>(),
                minCount: 2,
                maxCount: 3
            ));

            // 6. 销毁爆发 — 碰到物块/超时销毁时，全向泼洒 WaterDropProj + 水花 Dust
            Behaviors.Add(new ParticleBurstBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Count = 10,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 5f,
                AngleSpread = MathHelper.TwoPi,
                SpawnExtraDust = true,
                ExtraDustCount = 8,
                DustType = DustID.Water,
                DustColorStart = new Color(60, 180, 255, 150),
                DustColorEnd = new Color(60, 180, 255, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = true,
            });

            // 7. 环境光 — 水球微光
            Behaviors.Add(new GlowLightBehavior(new Vector3(0.05f, 0.15f, 0.3f)));

            // 8. 阻止默认贴图绘制
            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
