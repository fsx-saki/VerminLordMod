using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 影道基础弹幕 — 暗影突袭。
    ///
    /// 设计哲学：
    /// 影道的本质是"隐匿 + 突袭 + 致命"。弹幕以高速追踪敌人，
    /// 命中后产生环形暗影冲击波（SplashBehavior Ring 模式），
    /// 视觉上以影系拖尾（触手 + 池 + 影分身）和低可视度模拟暗影的隐匿突袭感。
    ///
    /// 与 DarkBaseProj 的区别：
    /// - Dark 是"吞噬腐蚀"，偏魔法/诅咒
    /// - Shadow 是"隐匿突袭"，偏物理/暗杀
    ///
    /// 运动方式：
    /// - 高速追踪敌人（HomingBehavior）
    /// - 命中后环形暗影冲击波
    ///
    /// 视觉效果：
    /// - 影系拖尾：触手 + 池 + 影分身（ShadowTrailBehavior）
    /// - 命中时环形暗影冲击波（SplashBehavior Ring 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 高速追踪敌人
    /// - ShadowTrailBehavior: 影系拖尾（触手 + 池 + 影分身）
    /// - SplashBehavior(Ring): 命中时环形暗影冲击波
    /// </summary>
    public class ShadowBaseProj : BaseBullet
    {
        private const float FlySpeed = 12f;
        private const float TrackWeight = 1f / 10f;
        private const int MaxLife = 200;

        protected override void RegisterBehaviors()
        {
            // 1. 高速追踪敌人
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 700f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 2. 影道拖尾（触手 + 池 + 影分身）
            Behaviors.Add(new ShadowTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(30, 25, 50, 160),
                TendrilColor = new Color(40, 30, 70, 200),
                PoolColor = new Color(25, 20, 45, 180),
                CloneColor = new Color(50, 40, 80, 180),
            });

            // 4. 命中时环形暗影冲击波
            Behaviors.Add(new SplashBehavior(SplashMode.Ring)
            {
                Count = 8,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 4f,
                RingAngleOffset = 0.25f,
                SpawnExtraDust = true,
                ExtraDustCount = 10,
                DustType = DustID.Ash,
                DustColorStart = new Color(60, 60, 80, 180),
                DustColorEnd = new Color(20, 20, 30, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 1f,
                DustSpeedMax = 3f,
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
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Darkness, 120);
            target.AddBuff(BuffID.Blackout, 60);
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
                    DustID.Ash,
                    vel,
                    0,
                    new Color(50, 50, 70, 150),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}