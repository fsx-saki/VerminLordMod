using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 金道基础弹幕 — 金锋刃。
    ///
    /// 设计哲学：
    /// 金的本质是"锋利 + 反弹 + 破甲"。弹幕以高速直线飞行，
    /// 撞墙反弹（BounceBehavior），命中后产生径向金属碎片爆散
    /// （SplashBehavior Radial 模式），视觉上以金色火花拖尾
    /// 和金属光泽模拟利刃的锋锐感。
    ///
    /// 运动方式：
    /// - 高速直线飞行（AimBehavior）
    /// - 撞墙反弹（BounceBehavior，最多3次）
    /// - 命中后径向金属碎片爆散
    ///
    /// 视觉效果：
    /// - 金色火花粒子拖尾
    /// - 金色发光
    /// - 命中时径向金属碎片爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 高速直线飞行
    /// - BounceBehavior: 撞墙反弹
    /// - DustTrailBehavior: 火花粒子拖尾
    /// - GlowDrawBehavior: 金色发光
    /// - SplashBehavior(Radial): 命中时径向金属碎片爆散
    /// </summary>
    public class MetalBaseProj : BaseBullet
    {
        private const float FlySpeed = 13f;
        private const int MaxLife = 200;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.7f, 0.6f, 0.2f)
            });

            Behaviors.Add(new BounceBehavior(maxBounces: 3, bounceFactor: 0.6f)
            {
                KillOnMaxBounces = true,
                TriggerKillOnMaxBounces = true,
                StopOnLowSpeed = true,
                LowSpeedThreshold = 1f,
                TimeLeftAfterStop = 20
            });

            Behaviors.Add(new DustTrailBehavior(DustID.GoldCoin, spawnChance: 1)
            {
                DustScale = 0.5f,
                VelocityMultiplier = 0.06f,
                NoGravity = true,
                DustAlpha = 180,
                RandomSpeed = 0.25f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 200, 50, 180),
                GlowBaseScale = 1.2f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.7f, 0.2f)
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 12,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 4f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.GoldCoin,
                DustColorStart = new Color(255, 200, 50, 220),
                DustColorEnd = new Color(200, 150, 30, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.6f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Ichor, 120);
            target.AddBuff(BuffID.BrokenArmor, 180);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.GoldCoin,
                    vel,
                    0,
                    new Color(255, 200, 50, 180),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}