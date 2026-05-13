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
    /// - 魂系拖尾：灵焰 + 锁链 + 鬼火（SoulTrailBehavior）
    /// - 命中时径向灵魂爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 慢速追踪敌人
    /// - SoulTrailBehavior: 魂系拖尾（灵焰 + 锁链 + 鬼火）
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

            Behaviors.Add(new SoulTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(120, 180, 255, 160),
                FlameColor = new Color(140, 200, 255, 230),
                ChainColor = new Color(100, 160, 240, 200),
                WispColor = new Color(180, 220, 255, 240),
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