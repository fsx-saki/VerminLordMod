using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 雷道基础弹幕 — 雷电链击。
    ///
    /// 设计哲学：
    /// 雷电的本质是"高速 + 跳跃 + 连锁"。弹幕以极快速度直线飞行，
    /// 命中敌人后电弧跳跃到附近敌人（连锁闪电），视觉上以锯齿形
    /// 轨迹和强烈闪光模拟雷电的狂暴感。
    ///
    /// 运动方式：
    /// - 极高速直线飞行（雷的速度感）
    /// - 命中后电弧跳跃到附近敌人（最多 3 次连锁）
    ///
    /// 视觉效果：
    /// - 雷电粒子拖尾（黄色/白色闪电粒子）
    /// - 强烈发光（闪电的刺眼感）
    /// - 命中时锥形电弧喷射（SplashBehavior Cone 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 极高速直线飞行
    /// - DustTrailBehavior: 雷电粒子拖尾
    /// - GlowDrawBehavior: 强烈发光
    /// - SplashBehavior(Cone): 命中时锥形电弧喷射
    /// </summary>
    public class LightningBaseProj : BaseBullet
    {
        private const float FlySpeed = 18f;
        private const int MaxLife = 60;
        private const int ChainRange = 300;
        private const int MaxChains = 3;

        private int _chainCount = 0;

        protected override void RegisterBehaviors()
        {
            // 1. 极高速直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.8f, 0.3f)
            });

            // 2. 雷电粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Electric, spawnChance: 1)
            {
                DustScale = 0.8f,
                VelocityMultiplier = 0.15f,
                NoGravity = true,
                DustAlpha = 200,
                RandomSpeed = 0.5f
            });

            // 3. 强烈发光
            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 255, 150, 200),
                GlowBaseScale = 1.5f,
                GlowLayers = 3,
                GlowAlphaMultiplier = 0.4f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 1.0f, 0.4f)
            });

            // 4. 命中时锥形电弧喷射
            Behaviors.Add(new SplashBehavior(SplashMode.Cone)
            {
                Count = 8,
                SpeedMin = 4f,
                SpeedMax = 10f,
                SpreadRadius = 4f,
                ConeAngle = 0.4f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Electric,
                DustColorStart = new Color(255, 255, 100, 220),
                DustColorEnd = new Color(255, 200, 50, 0),
                DustScaleMin = 0.5f,
                DustScaleMax = 1.0f,
                DustSpeedMin = 2f,
                DustSpeedMax = 6f,
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
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 电弧连锁：命中后跳跃到附近敌人
            if (_chainCount < MaxChains)
            {
                ChainLightning(target);
            }
        }

        private void ChainLightning(NPC fromTarget)
        {
            NPC closestNPC = null;
            float closestDist = ChainRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc == null || !npc.active || npc.friendly || npc == fromTarget)
                    continue;
                if (npc.life <= 0 || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(fromTarget.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNPC = npc;
                }
            }

            if (closestNPC != null)
            {
                _chainCount++;

                // 生成电弧弹幕飞向目标
                Vector2 direction = (closestNPC.Center - fromTarget.Center).SafeNormalize(Vector2.Zero);
                Vector2 spawnPos = fromTarget.Center + direction * 20f;

                Projectile chain = Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    direction * FlySpeed * 0.8f,
                    Projectile.type,
                    (int)(Projectile.damage * 0.7f),
                    Projectile.knockBack * 0.7f,
                    Projectile.owner
                );

                if (chain != null && chain.ModProjectile is LightningBaseProj chainProj)
                {
                    chainProj._chainCount = _chainCount;
                }

                // 电弧视觉特效
                for (int i = 0; i < 15; i++)
                {
                    float t = i / 15f;
                    Vector2 pos = Vector2.Lerp(fromTarget.Center, closestNPC.Center, t);
                    pos += Main.rand.NextVector2Circular(8f, 8f);

                    Dust d = Dust.NewDustPerfect(
                        pos,
                        DustID.Electric,
                        Vector2.Zero,
                        0,
                        new Color(255, 255, 150, 200),
                        Main.rand.NextFloat(0.5f, 1.2f)
                    );
                    d.noGravity = true;
                    d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                }
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 额外闪电闪光
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Electric,
                    vel,
                    0,
                    new Color(255, 255, 100, 200),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}