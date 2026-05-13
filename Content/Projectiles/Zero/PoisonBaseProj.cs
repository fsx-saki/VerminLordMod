using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 毒道基础弹幕 — 毒雾扩散。
    ///
    /// 设计哲学：
    /// 毒道的本质是"扩散 + 持续 + 腐蚀"。弹幕以中速飞行，
    /// 命中后产生径向毒雾爆散（SplashBehavior Radial 模式），
    /// 视觉上以绿色毒雾拖尾和腐蚀性粒子模拟毒素的蔓延感。
    ///
    /// 运动方式：
    /// - 中速直线飞行
    /// - 命中后径向毒雾爆散
    ///
    /// 视觉效果：
    /// - 毒系拖尾：孢子 + 腐蚀滴 + 瘴气（PoisonTrailBehavior）
    /// - 命中时径向毒雾爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 中速直线飞行
    /// - PoisonTrailBehavior: 毒系拖尾（孢子 + 腐蚀滴 + 瘴气）
    /// - SplashBehavior(Radial): 命中时径向毒雾爆散
    /// </summary>
    public class PoisonBaseProj : BaseBullet
    {
        private const float FlySpeed = 8f;
        private const int MaxLife = 180;

        protected override void RegisterBehaviors()
        {
            // 1. 中速直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.1f, 0.4f, 0.1f)
            });

            Behaviors.Add(new PoisonTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(80, 180, 60, 140),
                SporeBubbleColor = new Color(100, 200, 60, 200),
                CorrosionDripColor = new Color(120, 220, 50, 220),
                MiasmaCloudColor = new Color(60, 160, 40, 160),
            });

            // 4. 命中时径向毒雾爆散
            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 12,
                SpeedMin = 2f,
                SpeedMax = 5f,
                SpreadRadius = 6f,
                SpawnExtraDust = true,
                ExtraDustCount = 15,
                DustType = DustID.Poisoned,
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
            target.AddBuff(BuffID.Poisoned, 300);
            target.AddBuff(BuffID.Venom, 180);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Poisoned,
                    vel,
                    0,
                    new Color(30, 150, 30, 150),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}