using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 光道基础弹幕 — 光棱折射。
    ///
    /// 设计哲学：
    /// 光道的本质是"穿透 + 折射 + 净化"。弹幕以极高速穿透敌人，
    /// 命中后向前方分裂出小型光棱（SplashBehavior Forward 模式），
    /// 视觉上以金色/白色光芒和强烈发光模拟神圣光束的穿透感。
    ///
    /// 运动方式：
    /// - 极高速直线飞行，高穿透
    /// - 命中后向前方分裂小型光棱
    ///
    /// 视觉效果：
    /// - 金色粒子拖尾
    /// - 强烈白色发光
    /// - 命中时前方光棱分裂（SplashBehavior Forward 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 极高速直线飞行
    /// - DustTrailBehavior: 金色粒子拖尾
    /// - GlowDrawBehavior: 强烈白色发光
    /// - SplashBehavior(Forward): 命中时前方光棱分裂
    /// </summary>
    public class LightBaseProj : BaseBullet
    {
        private const float FlySpeed = 16f;
        private const int MaxLife = 90;

        protected override void RegisterBehaviors()
        {
            // 1. 极高速直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 1.0f, 0.8f)
            });

            // 2. 金色粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.YellowStarDust, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                DustAlpha = 200,
                RandomSpeed = 0.3f
            });

            // 3. 强烈白色发光
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 255, 200, 200),
                GlowBaseScale = 1.4f,
                GlowLayers = 3,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 1.2f, 0.9f)
            });

            // 4. 命中时前方光棱分裂
            Behaviors.Add(new SplashBehavior(SplashMode.Forward)
            {
                Count = 6,
                SpeedMin = 5f,
                SpeedMax = 10f,
                SpreadRadius = 3f,
                ConeAngle = 0.3f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.YellowStarDust,
                DustColorStart = new Color(255, 255, 150, 220),
                DustColorEnd = new Color(255, 200, 100, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.8f,
                DustSpeedMin = 2f,
                DustSpeedMax = 5f,
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
            Projectile.penetrate = 3;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.YellowStarDust,
                    vel,
                    0,
                    new Color(255, 255, 200, 180),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}