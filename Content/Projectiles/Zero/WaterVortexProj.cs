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
    /// 水道漩涡 — 吸附敌人的漩涡弹幕。
    /// 水道技术储备库的"漩涡/吸附"技术：
    /// - 在指定位置生成漩涡，固定不动
    /// - 持续吸附范围内的敌人向中心拉拽（使用 PullBehavior）
    /// - 对范围内的敌人造成持续伤害（使用 AreaDamageBehavior）
    /// - 产生大量水粒子在圆形范围内随机散布，旋转汇聚向中心（使用 VortexParticleBehavior Cloud 模式）
    /// - 持续一段时间后消失
    ///
    /// 行为组合（完全解耦）：
    /// - StationaryBehavior: 固定位置
    /// - FadeInOutBehavior: 渐入渐出
    /// - PullBehavior: 吸附敌人（大吸力 + 旋转效果）
    /// - AreaDamageBehavior: 范围伤害
    /// - VortexParticleBehavior (Cloud): 浓密旋转汇聚粒子
    /// - DropletSplashBehavior: 定时向上泼洒液滴
    /// </summary>
    public class WaterVortexProj : BaseBullet
    {
        /// <summary>漩涡持续时间（帧）</summary>
        private const int Duration = 240; // 4秒

        /// <summary>吸附范围（像素）</summary>
        private const float VortexRange = 200f;

        /// <summary>吸附力强度（大吸力）</summary>
        private const float PullStrength = 0.3f;

        /// <summary>切向旋转因子</summary>
        private const float TangentFactor = 0.6f;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 90f;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 8;

        protected override void RegisterBehaviors()
        {
            // 1. 固定位置 — 漩涡生成后不移动
            Behaviors.Add(new StationaryBehavior());

            // 2. 渐入渐出
            Behaviors.Add(new FadeInOutBehavior
            {
                FadeInDuration = 0.15f,
                FadeOutStart = 0.75f,
                MaxAlpha = 0,
                MinAlpha = 180,
                TotalLife = Duration
            });

            // 3. 吸附敌人 — 大吸力 + 旋转效果
            Behaviors.Add(new PullBehavior
            {
                PullRange = VortexRange,
                PullStrength = PullStrength,
                TangentFactor = TangentFactor,
                MaxPullSpeed = 12f,
                EnableLight = false,
            });

            // 4. 范围伤害
            Behaviors.Add(new AreaDamageBehavior
            {
                HitRadius = HitRadius,
                HitInterval = HitInterval,
                Knockback = 0f,
                UseLocalNPCHitCooldown = true,
                AutoSetCooldown = true,
                DirectionalKnockback = true,
            });

            // 5. 浓密旋转汇聚粒子 — Cloud 模式：大量粒子在圆形范围内随机散布，旋转并汇聚向中心
            Behaviors.Add(new VortexParticleBehavior
            {
                UseCloudMode = true,
                CloudParticleCount = 25,
                CloudRadius = 70f,
                CloudRotationSpeed = 0.07f,
                CloudConvergenceSpeed = 0.05f,
                CloudInnerBias = 1.4f,
                CloudScaleRange = new Vector2(0.35f, 0.85f),

                CloudStreamerCount = 18,
                CloudStreamerColor = new Color(15, 50, 160, 220),
                CloudStreamerScale = new Vector2(0.8f, 1.4f),
                CloudStreamerArms = 3,
                CloudStreamerTightness = 0.025f,
                CloudStreamerWidth = 10f,

                DustType = DustID.Water,
                ColorStart = new Color(40, 140, 220, 0),
                ColorEnd = new Color(120, 230, 255, 0),

                SpawnCenterGlow = true,
                CenterGlowInterval = 2,
                CenterGlowRange = 20f,

                SpawnBubbles = true,
                BubbleInterval = 3,
                BubbleRange = 55f,

                EnableLight = true,
                LightColor = new Vector3(0.15f, 0.4f, 0.8f),
            });

            // 6. 液滴泼洒 — 定时从中心向上方泼洒 WaterDropProj
            Behaviors.Add(new DropletSplashBehavior
            {
                ChildProjectileType = ModContent.ProjectileType<WaterDropProj>(),
                Interval = 15,
                Count = 3,
                SpreadX = 30f,
                SpeedYMin = -3f,
                SpeedYMax = -1f,
                SpawnOffsetY = -10f,
                HorizontalSpeedMultiplier = 0.15f,
            });

            // 7. 销毁粉尘爆发 — 消失时产生多层水花爆裂
            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new System.Collections.Generic.List<KillDustBurstBehavior.DustBurstLayer>
                {
                    // 层1：无重力水花（大粒子，高速）
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 20,
                        DustType = DustID.Water,
                        Color = new Color(80, 200, 255, 200),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.6f,
                        NoGravity = true,
                        SpreadRadius = 10f,
                        UseCircularVelocity = true,
                        SpeedMin = 2f,
                        SpeedMax = 7f,
                    },
                    // 层2：有重力水滴（小粒子，下落）
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 8,
                        DustType = DustID.Water,
                        Color = new Color(60, 180, 255, 150),
                        Alpha = 50,
                        ScaleMin = 0.4f,
                        ScaleMax = 0.8f,
                        NoGravity = false,
                        SpreadRadius = 8f,
                        UseCircularVelocity = false,
                        VelXMin = -3f,
                        VelXMax = 3f,
                        VelYMin = -4f,
                        VelYMax = -1f,
                    },
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = HitInterval * 2;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
        }
    }
}
