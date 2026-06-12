using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WindCycloneProj : BaseBullet
    {
        private const int Duration = 180;
        private const float OrbitRadius = 50f;
        private const float OrbitSpeed = 0.12f;

        private Vector2 _center;
        private float _angle;

        protected override void RegisterBehaviors()
        {
            // 不使用StationaryBehavior — 弹幕自身绕中心旋转以产生风卷残云轨迹

            Behaviors.Add(new PullBehavior
            {
                PullRange = 200f,
                PullStrength = 0.25f,
                TangentFactor = 0.7f,
                MaxPullSpeed = 7f,
                EnableLight = false,
            });

            Behaviors.Add(new AreaDamageBehavior
            {
                HitRadius = 70f,
                HitInterval = 12,
                Knockback = 4f,
                DirectionalKnockback = true,
            });

            // 风卷残云式粒子特效 — 大旋风版
            Behaviors.Add(new WindTrailBehavior
            {
                EnableGhostTrail = true,
                GhostMaxPositions = 25,
                GhostRecordInterval = 2,
                GhostWidthScale = 0.4f,
                GhostLengthScale = 2.0f,
                GhostAlpha = 0.35f,
                GhostColor = new Color(160, 240, 220, 180),
                MaxStreaks = 60,
                StreakLife = 25,
                StreakSize = 0.7f,
                StreakStretch = 3.5f,
                StreakDrift = 0.5f,
                StreakColor = new Color(160, 230, 210, 220),
                MaxVortex = 45,
                VortexLife = 35,
                VortexSize = 0.55f,
                VortexRotSpeed = 0.12f,
                VortexExpandRate = 2.0f,
                VortexDriftSpeed = 0.8f,
                VortexColor = new Color(140, 220, 200, 200),
                MaxMist = 25,
                MistLife = 50,
                MistStartSize = 0.5f,
                MistEndSize = 2.5f,
                MistSpawnChance = 0.22f,
                MistDriftSpeed = 0.5f,
                MistColor = new Color(180, 240, 230, 130),
                InertiaFactor = 0.3f,
                RandomSpread = 6f,
                AutoDraw = true,
                SuppressDefaultDraw = true,
            });

            Behaviors.Add(new FadeInOutBehavior
            {
                FadeInDuration = 0.08f,
                FadeOutStart = 0.85f,
                MinAlpha = 255,
                MaxAlpha = 0,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _center = Projectile.Center;
            _angle = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void OnAI()
        {
            // 弹幕绕中心点旋转，产生风卷残云轨迹
            _angle += OrbitSpeed;
            Vector2 offset = _angle.ToRotationVector2() * OrbitRadius;
            Projectile.Center = _center + offset;
            Projectile.velocity = offset.RotatedBy(MathHelper.PiOver2) * OrbitSpeed;

            // 中心点发光
            Lighting.AddLight(_center, 0.25f, 0.55f, 0.45f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 90);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 25; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 7f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    _center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.Cloud, vel, 0,
                    new Color(160, 230, 210, 160),
                    Main.rand.NextFloat(0.6f, 1.2f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
