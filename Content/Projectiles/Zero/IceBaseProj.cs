using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 冰道基础弹幕 — 寒冰碎片。
    ///
    /// 设计哲学：
    /// 冰的本质是"冻结 + 碎裂 + 减速"。弹幕以中速直线飞行，
    /// 命中后沿法线方向爆散冰晶碎片（SplashBehavior Normal 模式），
    /// 视觉上以蓝白色冰晶拖尾和冷光模拟极寒。
    ///
    /// 运动方式：
    /// - 中速直线飞行（AimBehavior）
    /// - 命中后法线方向冰晶爆散
    ///
    /// 视觉效果：
    /// - 蓝白色冰晶粒子拖尾
    /// - 冰蓝色发光
    /// - 命中时法线冰晶爆散（SplashBehavior Normal 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 中速直线飞行
    /// - DustTrailBehavior: 冰晶粒子拖尾
    /// - GlowDrawBehavior: 冰蓝色发光
    /// - SplashBehavior(Normal): 命中时法线冰晶爆散
    /// </summary>
    public class IceBaseProj : BaseBullet
    {
        private const float FlySpeed = 10f;
        private const int MaxLife = 180;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 0.9f)
            });

            Behaviors.Add(new DustTrailBehavior(DustID.IceTorch, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.08f,
                NoGravity = true,
                DustAlpha = 180,
                RandomSpeed = 0.3f
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(100, 180, 255, 180),
                GlowBaseScale = 1.3f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 0.9f)
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Normal)
            {
                Count = 10,
                SpeedMin = 2f,
                SpeedMax = 6f,
                SpreadRadius = 4f,
                NormalSpreadAngle = 0.6f,
                SpawnExtraDust = true,
                ExtraDustCount = 14,
                DustType = DustID.IceTorch,
                DustColorStart = new Color(150, 210, 255, 220),
                DustColorEnd = new Color(50, 100, 200, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
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
            target.AddBuff(BuffID.Frostburn, 180);
            target.AddBuff(BuffID.Chilled, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.IceTorch,
                    vel,
                    0,
                    new Color(150, 210, 255, 180),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}