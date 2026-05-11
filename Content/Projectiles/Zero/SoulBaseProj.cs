using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 魂道基础弹幕 — 灵魂追踪。
    ///
    /// 设计哲学：
    /// 魂道的本质是"追踪 + 穿墙 + 虚无"。弹幕以慢速追踪敌人，
    /// 可以穿透物块（灵魂不受物理阻碍），命中后产生径向灵魂爆散
    /// （SplashBehavior Radial 模式），视觉上以蓝白色幽灵粒子
    /// 和虚无感模拟灵魂的飘渺。
    ///
    /// 运动方式：
    /// - 慢速追踪敌人（HomingBehavior）
    /// - 穿透物块（tileCollide = false）
    ///
    /// 视觉效果：
    /// - 蓝白色幽灵粒子拖尾
    /// - 淡蓝色发光
    /// - 命中时径向灵魂爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 慢速追踪敌人
    /// - DustTrailBehavior: 幽灵粒子拖尾
    /// - GlowDrawBehavior: 淡蓝色发光
    /// - SplashBehavior(Radial): 命中时径向灵魂爆散
    /// </summary>
    public class SoulBaseProj : BaseBullet
    {
        private const float FlySpeed = 6f;
        private const float TrackWeight = 1f / 20f;
        private const int MaxLife = 360;

        protected override void RegisterBehaviors()
        {
            // 1. 慢速追踪敌人
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 1000f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 2. 幽灵粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.SpectreStaff, spawnChance: 1)
            {
                DustScale = 0.6f,
                VelocityMultiplier = 0.05f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.2f
            });

            // 3. 淡蓝色发光
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 200, 255, 150),
                GlowBaseScale = 1.3f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.25f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.4f, 0.8f)
            });

            // 4. 命中时径向灵魂爆散
            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 8,
                SpeedMin = 1f,
                SpeedMax = 4f,
                SpreadRadius = 4f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.SpectreStaff,
                DustColorStart = new Color(120, 180, 255, 200),
                DustColorEnd = new Color(40, 60, 150, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 0.5f,
                DustSpeedMax = 2.5f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Confused, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.SpectreStaff,
                    vel,
                    0,
                    new Color(120, 180, 255, 150),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}