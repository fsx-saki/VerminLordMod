using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WindBaseProj : BaseBullet
    {
        private const float OutwardSpeed = 14f;
        private const float ReturnSpeed = 16f;
        private const int OutwardFrames = 25;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new BoomerangBehavior(OutwardSpeed, ReturnSpeed, OutwardFrames)
            {
                SpinSpeed = 0.4f,
                ReturnAccel = 0.6f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.5f),
            });

            Behaviors.Add(new WindTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.35f,
                GhostMaxPositions = 12,
                MaxStreaks = 45,
                StreakLife = 14,
                StreakSize = 0.45f,
                StreakStretch = 2.5f,
                StreakDrift = 0.2f,
                MaxVortex = 25,
                VortexLife = 28,
                VortexSize = 0.35f,
                VortexRotSpeed = 0.1f,
                MaxMist = 8,
                MistLife = 30,
                MistSpawnChance = 0.08f,
                AutoDraw = true,
                SuppressDefaultDraw = true,
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 240, 210, 100),
                GlowBaseScale = 1.3f,
                GlowLayers = 2,
                GlowAlphaMultiplier = 0.25f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.7f, 0.5f)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 60);

            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1.5f, 3.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Cloud, vel, 0,
                    new Color(180, 240, 220, 180),
                    Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 2.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, vel, 0,
                    new Color(160, 230, 210, 150), Main.rand.NextFloat(0.4f, 0.8f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
