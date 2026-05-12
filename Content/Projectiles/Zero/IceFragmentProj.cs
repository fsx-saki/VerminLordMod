using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 冰碎片弹幕 — 冰晶/冰锥碎裂时爆出的碎片。
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior: 冰色粒子体
    /// - IceTrailBehavior: 短冰系拖尾
    /// - GravityBehavior: 受重力下落
    /// - DustKillBehavior: 消失时冰尘爆散
    ///
    /// 区别于 WaterDropProj：不反弹，碰到物块直接碎裂。
    /// </summary>
    public class IceFragmentProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 碎片大小减小（bodyRadius 6→4, ParticleSize 0.5→0.35）
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 6, bodyRadius: 4f)
            {
                ParticleSize = 0.35f,
                ColorStart = new Color(160, 220, 255, 200),
                ColorEnd = new Color(160, 220, 255, 200),
                SwirlSpeed = 0.04f,
                ReturnForce = 0.6f,
                JitterStrength = 0.2f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.2f,
                EnableLight = false,
            });

            // 碎片拖尾长宽都减小（StarSize 0.3→0.18, SnowflakeSize 0.15→0.08）
            Behaviors.Add(new IceTrailBehavior
            {
                MaxStars = 6,
                StarLife = 12,
                StarSpawnInterval = 5,
                StarSize = 0.18f,
                MaxSnowflakes = 12,
                SnowflakeLife = 12,
                SnowflakeSize = 0.08f,
                SnowflakeClusterSize = 2,
                SnowflakeSpawnChance = 0.3f,
                SnowflakeGravity = 0.03f,
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });

            Behaviors.Add(new GravityBehavior(acceleration: 0.2f, maxFallSpeed: 8f)
            {
                AutoRotate = true,
                RotationOffset = 0f,
            });

            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Ice,
                dustCount: 5,
                dustSpeed: 2f,
                dustScale: 0.6f
            )
            {
                NoGravity = false
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            // 碎片大小减小（8→6, scale 0.5→0.35）
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.35f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            Lighting.AddLight(Projectile.Center, 0.15f, 0.35f, 0.6f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 60);
        }
    }
}