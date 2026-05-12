using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 水道治疗弹 — 青色粒子体小球，飞向鼠标。
    ///
    /// 命中敌人时碎裂为青色液滴，并概率生成 1-3 颗治疗追踪弹飞向友方。
    /// 碰到物块时碎裂为青色液滴。
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior: 青色粒子小球
    /// - WaterTrailBehavior: 短青色拖尾
    /// - AimBehavior: 飞向鼠标
    /// - HealSeekerSpawnBehavior: 命中敌人概率生成治疗追踪弹
    /// - ParticleBurstBehavior: 碎裂时青色液滴散落
    /// - SuppressDrawBehavior: 阻止默认贴图
    /// </summary>
    public class WaterHealProj : BaseBullet
    {
        private const float FlySpeed = 8f;
        private const float SeekerSpawnChance = 0.7f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 14, bodyRadius: 10f)
            {
                ParticleSize = 0.7f,
                ColorStart = new Color(80, 220, 255, 220),
                ColorEnd = new Color(80, 220, 255, 220),
                SwirlSpeed = 0.08f,
                ReturnForce = 0.8f,
                JitterStrength = 0.5f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.4f,
                EnableLight = false,
            });

            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 18,
                ParticleLife = 10,
                SizeMultiplier = 0.3f,
                SpawnChance = 0.5f,
                SplashSpeed = 0.15f,
                SplashAngle = 0.12f,
                InertiaFactor = 0.02f,
                RandomSpread = 0.5f,
                Gravity = 0.1f,
                AirResistance = 0.97f,
                BubbleChance = 0.25f,
                BubbleSizeMultiplier = 1.3f,
                ColorStart = new Color(80, 220, 255, 180),
                ColorEnd = new Color(80, 220, 255, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false,
                AdaptiveLife = true,
                AdaptiveTargetLength = 40f,
                SpeedLifeExponent = 0.3f,
                MinParticleLife = 4,
            });

            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new HealSeekerSpawnBehavior(
                ModContent.ProjectileType<WaterHealSeekerProj>(),
                SeekerSpawnChance,
                minCount: 1,
                maxCount: 3
            )
            {
                Speed = 2f,
            });

            Behaviors.Add(new ParticleBurstBehavior
            {
                ChildProjectileType = -1,
                Count = 0,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.HealingPlus,
                DustColorStart = new Color(80, 220, 255, 200),
                DustColorEnd = new Color(80, 220, 255, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = false,
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}