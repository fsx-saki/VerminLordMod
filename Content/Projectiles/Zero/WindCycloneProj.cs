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

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new StationaryBehavior());

            Behaviors.Add(new PullBehavior
            {
                PullRange = 160f,
                PullStrength = 0.2f,
                TangentFactor = 0.6f,
                MaxPullSpeed = 6f,
                EnableLight = false,
            });

            Behaviors.Add(new AreaDamageBehavior
            {
                HitRadius = 50f,
                HitInterval = 15,
                Knockback = 3f,
                DirectionalKnockback = true,
            });

            Behaviors.Add(new VortexParticleBehavior
            {
                UseCloudMode = true,
                CloudParticleCount = 15,
                CloudRadius = 55f,
                CloudRotationSpeed = 0.08f,
                CloudConvergenceSpeed = 0.01f,
                CloudInnerBias = 1.5f,
                CloudStreamerCount = 8,
                CloudStreamerArms = 3,
                CloudStreamerTightness = 0.04f,
                CloudStreamerWidth = 10f,
                CloudStreamerColor = new Color(140, 220, 200, 220),
                CloudStreamerScale = new Vector2(0.6f, 1.1f),
                DustType = DustID.Cloud,
                ColorStart = new Color(160, 230, 210, 150),
                ColorEnd = new Color(200, 250, 240, 200),
                SpawnBubbles = false,
                SpawnCenterGlow = true,
                CenterGlowInterval = 4,
                CenterGlowRange = 12f,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.5f, 0.4f),
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
            Projectile.localNPCHitCooldown = 15;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 90);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Cloud, vel, 0,
                    new Color(160, 230, 210, 160),
                    Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
