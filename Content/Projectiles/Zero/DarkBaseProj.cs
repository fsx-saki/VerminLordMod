using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 暗道基础弹幕 — 暗影吞噬。
    ///
    /// 设计哲学：
    /// 暗道的本质是"吞噬 + 腐蚀 + 扩散"。弹幕以追踪方式逼近敌人，
    /// 命中后产生环形暗影冲击波（SplashBehavior Ring 模式），
    /// 视觉上以暗系拖尾（暗斑 + 锁链 + 诅咒印记）模拟暗影的压迫感。
    ///
    /// 运动方式：
    /// - 追踪敌人（HomingBehavior）
    /// - 命中后环形暗影冲击波扩散
    ///
    /// 视觉效果：
    /// - 暗系拖尾：暗斑 + 锁链 + 诅咒印记（DarkTrailBehavior）
    /// - 命中时环形暗影冲击波（SplashBehavior Ring 模式）
    ///
    /// 行为组合：
    /// - HomingBehavior: 追踪敌人
    /// - DarkTrailBehavior: 暗系拖尾（暗斑 + 锁链 + 诅咒印记）
    /// - SplashBehavior(Ring): 命中时环形暗影冲击波
    /// </summary>
    public class DarkBaseProj : BaseBullet
    {
        private const float FlySpeed = 10f;
        private const float TrackWeight = 1f / 11f;
        private const int MaxLife = 240;

        protected override void RegisterBehaviors()
        {
            // 1. 追踪敌人
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new DarkTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(60, 15, 90, 160),
                PatchColor = new Color(50, 10, 80, 180),
                ChainColor = new Color(80, 25, 120, 200),
                CurseMarkColor = new Color(100, 30, 150, 220),
            });

            // 4. 命中时环形暗影冲击波
            Behaviors.Add(new SplashBehavior(SplashMode.Ring)
            {
                Count = 10,
                SpeedMin = 3f,
                SpeedMax = 7f,
                SpreadRadius = 5f,
                RingAngleOffset = 0.3f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Shadowflame,
                DustColorStart = new Color(100, 30, 150, 200),
                DustColorEnd = new Color(40, 10, 60, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.9f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
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
            target.AddBuff(BuffID.ShadowFlame, 180);
            target.AddBuff(BuffID.Darkness, 120);
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
                    DustID.Shadowflame,
                    vel,
                    0,
                    new Color(80, 20, 120, 180),
                    Main.rand.NextFloat(0.5f, 0.9f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}