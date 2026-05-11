using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 虚空道基础弹幕 — 虚空之眼。
    ///
    /// 设计哲学：
    /// 虚空的本质是"吞噬 + 牵引 + 湮灭"。弹幕以慢速追踪敌人，
    /// 飞行过程中持续牵引周围敌人（PullBehavior），
    /// 命中后产生环形虚空冲击波（SplashBehavior Ring 模式），
    /// 视觉上以暗紫色虚空粒子拖尾和暗光模拟吞噬一切的虚无感。
    ///
    /// 运动方式：
    /// - 慢速追踪敌人（HomingBehavior）
    /// - 持续牵引周围敌人（PullBehavior）
    /// - 命中后环形虚空冲击波
    ///
    /// 视觉效果：
    /// - 暗紫色虚空粒子拖尾
    /// - 暗紫色发光
    /// - 命中时环形虚空冲击波（SplashBehavior Ring 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 慢速追踪敌人
    /// - PullBehavior: 持续牵引周围敌人
    /// - DustTrailBehavior: 虚空粒子拖尾
    /// - GlowDrawBehavior: 暗紫色发光
    /// - SplashBehavior(Ring): 命中时环形虚空冲击波
    /// </summary>
    public class VoidBaseProj : BaseBullet
    {
        private const float FlySpeed = 7f;
        private const float TrackWeight = 1f / 25f;
        private const int MaxLife = 300;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new PullBehavior(pullRange: 200f, pullStrength: 0.12f, tangentFactor: 0.3f)
            {
                MaxPullSpeed = 6f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.05f, 0.4f)
            });

            Behaviors.Add(new DustTrailBehavior(DustID.Shadowflame, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.06f,
                NoGravity = true,
                DustAlpha = 160,
                RandomSpeed = 0.25f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 30, 150, 180),
                GlowBaseScale = 1.5f,
                GlowLayers = 3,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.4f, 0.05f, 0.5f)
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Ring)
            {
                Count = 12,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 5f,
                RingAngleOffset = 0.3f,
                SpawnExtraDust = true,
                ExtraDustCount = 16,
                DustType = DustID.Shadowflame,
                DustColorStart = new Color(120, 40, 180, 220),
                DustColorEnd = new Color(40, 10, 60, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.9f,
                DustSpeedMin = 1f,
                DustSpeedMax = 5f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
            target.AddBuff(BuffID.Blackout, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Shadowflame,
                    vel,
                    0,
                    new Color(120, 40, 180, 200),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}