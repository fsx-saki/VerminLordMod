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
    /// 视觉上以光系拖尾（光线 + 棱镜 + 光晕）模拟神圣光束的穿透感。
    ///
    /// 运动方式：
    /// - 极高速直线飞行，高穿透
    /// - 命中后向前方分裂小型光棱
    ///
    /// 视觉效果：
    /// - 光系拖尾：光线 + 棱镜 + 光晕（LightTrailBehavior）
    /// - 命中时前方光棱分裂（SplashBehavior Forward 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 极高速直线飞行
    /// - LightTrailBehavior: 光系拖尾（光线 + 棱镜 + 光晕）
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

            // 2. 光道拖尾（光线 + 棱镜 + 光晕）
            Behaviors.Add(new LightTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(255, 255, 200, 200),
                RayColor = new Color(255, 255, 220, 240),
                HaloInnerColor = new Color(255, 255, 230, 220),
                HaloOuterColor = new Color(255, 240, 180, 160),
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