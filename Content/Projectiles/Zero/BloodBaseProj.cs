using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 血道基础弹幕 — 血爆。
    ///
    /// 设计哲学：
    /// 血道的本质是"爆发 + 吸血 + 溅射"。弹幕以中速飞行，
    /// 命中后产生径向血爆（SplashBehavior Radial 模式），
    /// 并生成吸血粒子飞回玩家恢复生命。
    ///
    /// 运动方式：
    /// - 中速直线飞行
    /// - 命中后径向血爆
    ///
    /// 视觉效果：
    /// - 红色血雾粒子拖尾
    /// - 暗红色发光
    /// - 命中时径向血爆（SplashBehavior Radial 模式）
    /// - 吸血粒子飞回玩家
    ///
    /// 行为组合：
    /// - AimBehavior: 中速直线飞行
    /// - DustTrailBehavior: 血雾粒子拖尾
    /// - GlowDrawBehavior: 暗红色发光
    /// - SplashBehavior(Radial): 命中时径向血爆
    /// </summary>
    public class BloodBaseProj : BaseBullet
    {
        private const float FlySpeed = 10f;
        private const int MaxLife = 180;

        protected override void RegisterBehaviors()
        {
            // 1. 中速直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.05f, 0.05f)
            });

            // 2. 血雾粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Blood, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.1f,
                NoGravity = false,
                DustAlpha = 180,
                RandomSpeed = 0.3f
            });

            // 3. 暗红色发光
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(200, 30, 30, 150),
                GlowBaseScale = 1.2f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.05f, 0.05f)
            });

            // 4. 命中时径向血爆
            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 14,
                SpeedMin = 2f,
                SpeedMax = 6f,
                SpreadRadius = 5f,
                SpawnExtraDust = true,
                ExtraDustCount = 16,
                DustType = DustID.Blood,
                DustColorStart = new Color(180, 20, 20, 220),
                DustColorEnd = new Color(80, 10, 10, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.9f,
                DustSpeedMin = 1f,
                DustSpeedMax = 4f,
                DustNoGravity = false,
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
            // 吸血：生成血珠飞回玩家
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
            {
                Player player = Main.player[Projectile.owner];
                if (player != null && player.active && !player.dead)
                {
                    SpawnLifeStealParticles(target.Center, player);
                }
            }
        }

        private void SpawnLifeStealParticles(Vector2 fromPos, Player player)
        {
            int healAmount = Math.Max(1, Projectile.damage / 20);

            for (int i = 0; i < 5; i++)
            {
                Vector2 spawnPos = fromPos + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 vel = (player.Center - spawnPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f);

                Dust d = Dust.NewDustPerfect(
                    spawnPos,
                    DustID.LifeDrain,
                    vel,
                    0,
                    new Color(255, 50, 50, 200),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }

            // 实际回血
            player.Heal(healAmount);
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
                    DustID.Blood,
                    vel,
                    0,
                    new Color(180, 20, 20, 180),
                    Main.rand.NextFloat(0.5f, 0.9f)
                );
                d.noGravity = false;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}