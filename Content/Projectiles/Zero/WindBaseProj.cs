using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 风道基础弹幕 — 风刃。
    ///
    /// 设计哲学：
    /// 风的本质是"穿透 + 击退 + 流动"。弹幕以高速正弦波轨迹飞行，
    /// 可穿透多个敌人，命中后产生锥形气流喷射（SplashBehavior Cone 模式），
    /// 视觉上以白色气流拖尾和淡绿光模拟风的流动感。
    ///
    /// 运动方式：
    /// - 高速正弦波飞行（AimBehavior + WaveBehavior）
    /// - 穿透多个敌人（penetrate = 3）
    /// - 命中后锥形气流喷射
    ///
    /// 视觉效果：
    /// - 白色气流粒子拖尾
    /// - 淡绿色发光
    /// - 命中时锥形气流喷射（SplashBehavior Cone 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 高速直线飞行
    /// - WaveBehavior: 正弦波轨迹
    /// - DustTrailBehavior: 气流粒子拖尾
    /// - GlowDrawBehavior: 淡绿色发光
    /// - SplashBehavior(Cone): 命中时锥形气流喷射
    /// </summary>
    public class WindBaseProj : BaseBullet
    {
        private const float FlySpeed = 14f;
        private const int MaxLife = 120;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.3f)
            });

            Behaviors.Add(new WaveBehavior(amplitude: 0.03f, frequency: 0.08f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new DustTrailBehavior(DustID.Cloud, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 120,
                RandomSpeed = 0.4f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(180, 255, 180, 120),
                GlowBaseScale = 1.2f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.2f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.3f)
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Cone)
            {
                Count = 8,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 3f,
                ConeAngle = 0.5f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.Cloud,
                DustColorStart = new Color(200, 255, 200, 180),
                DustColorEnd = new Color(100, 200, 100, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 60);
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
                    DustID.Cloud,
                    vel,
                    0,
                    new Color(200, 255, 200, 150),
                    Main.rand.NextFloat(0.5f, 1.0f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}