using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 木道基础弹幕 — 缠绕生长。
    ///
    /// 设计哲学：
    /// 木道的本质是"生长 + 缠绕 + 束缚"。弹幕以追踪方式逼近敌人，
    /// 命中后产生径向藤蔓爆散（SplashBehavior Radial 模式），
    /// 视觉上以绿色叶片拖尾和自然生长感模拟藤蔓的缠绕力。
    ///
    /// 运动方式：
    /// - 追踪敌人（HomingBehavior）
    /// - 命中后径向藤蔓爆散
    ///
    /// 视觉效果：
    /// - 绿色叶片粒子拖尾
    /// - 翠绿色发光
    /// - 命中时径向藤蔓爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 追踪敌人
    /// - DustTrailBehavior: 叶片粒子拖尾
    /// - GlowDrawBehavior: 翠绿色发光
    /// - SplashBehavior(Radial): 命中时径向藤蔓爆散
    /// </summary>
    public class WoodBaseProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const float TrackWeight = 1f / 15f;
        private const int MaxLife = 240;

        protected override void RegisterBehaviors()
        {
            // 1. 追踪敌人
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 600f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 2. 叶片粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Grass, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.08f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.25f
            });

            // 3. 翠绿色发光
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(50, 200, 50, 150),
                GlowBaseScale = 1.2f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.25f,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.5f, 0.1f)
            });

            // 4. 命中时径向藤蔓爆散
            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 10,
                SpeedMin = 2f,
                SpeedMax = 5f,
                SpreadRadius = 5f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Grass,
                DustColorStart = new Color(30, 160, 30, 200),
                DustColorEnd = new Color(10, 60, 10, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
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
            target.AddBuff(BuffID.Poisoned, 180);
            target.AddBuff(BuffID.Slow, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Grass,
                    vel,
                    0,
                    new Color(30, 150, 30, 150),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}