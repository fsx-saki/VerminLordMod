using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 治疗追踪弹 — 青色粒子体小球，追踪最近的友方单位，接触后治疗 5 点生命。
    ///
    /// 由 WaterHealProj 命中敌人时概率生成。
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior: 青色粒子小球
    /// - WaterTrailBehavior: 短青色拖尾
    /// - FriendlyHomingBehavior: 追踪友方
    /// - HealOnContactBehavior: 接触友方时治疗
    /// - ParticleBurstBehavior: 销毁时青色液滴散落
    /// - SuppressDrawBehavior: 阻止默认贴图
    /// </summary>
    public class WaterHealSeekerProj : BaseBullet
    {
        private const float HomingStrength = 0.1f;
        private const float MaxSpeed = 7f;
        private const float DetectionRange = 500f;
        private const int HealAmount = 5;
        private const int Lifetime = 180;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 8, bodyRadius: 5f)
            {
                ParticleSize = 0.5f,
                ColorStart = new Color(80, 220, 255, 200),
                ColorEnd = new Color(80, 220, 255, 200),
                SwirlSpeed = 0.06f,
                ReturnForce = 0.7f,
                JitterStrength = 0.3f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.3f,
                EnableLight = false,
            });

            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 10,
                ParticleLife = 8,
                SizeMultiplier = 0.25f,
                SpawnChance = 0.5f,
                SplashSpeed = 0.1f,
                SplashAngle = 0.1f,
                InertiaFactor = 0.02f,
                RandomSpread = 0.4f,
                Gravity = 0.1f,
                AirResistance = 0.97f,
                BubbleChance = 0.2f,
                BubbleSizeMultiplier = 1.2f,
                ColorStart = new Color(80, 220, 255, 180),
                ColorEnd = new Color(80, 220, 255, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false,
                AdaptiveLife = true,
                AdaptiveTargetLength = 30f,
                SpeedLifeExponent = 0.3f,
                MinParticleLife = 4,
            });

            Behaviors.Add(new FriendlyHomingBehavior(HomingStrength, MaxSpeed, DetectionRange)
            {
                ArriveDistance = 15f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new HealOnContactBehavior(HealAmount, 25f)
            {
                HealDustCount = 5,
                DustType = DustID.HealingPlus,
                DustColor = new Color(80, 220, 255, 200),
                DustScaleMin = 0.5f,
                DustScaleMax = 1.0f,
                DustSpeed = 2f,
            });

            Behaviors.Add(new ParticleBurstBehavior
            {
                ChildProjectileType = -1,
                Count = 0,
                SpawnExtraDust = true,
                ExtraDustCount = 6,
                DustType = DustID.HealingPlus,
                DustColorStart = new Color(80, 220, 255, 180),
                DustColorEnd = new Color(80, 220, 255, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.6f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = true,
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}