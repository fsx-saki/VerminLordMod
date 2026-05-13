using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class GoldBaseProj : BaseBullet
    {
        private const float FlySpeed = 10f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.6f, 0.2f),
            });

            Behaviors.Add(new GoldTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.4f,
                GhostMaxPositions = 10,
                GhostWidthScale = 0.22f,
                GhostLengthScale = 1.6f,
                GhostColor = new Color(255, 215, 80, 180),

                MaxShards = 35,
                ShardLife = 20,
                ShardSize = 0.5f,
                ShardStretch = 2.0f,
                ShardSpawnInterval = 1,
                ShardSpinSpeed = 0.15f,
                ShardDriftSpeed = 0.3f,
                ShardSpread = 4f,
                ShardColor = new Color(255, 220, 100, 220),

                MaxSparks = 20,
                SparkLife = 28,
                SparkSize = 0.5f,
                SparkSpawnChance = 0.18f,
                SparkDriftSpeed = 0.25f,
                SparkColor = new Color(255, 240, 160, 240),

                MaxRings = 5,
                RingLife = 38,
                RingStartSize = 0.3f,
                RingEndSize = 1.4f,
                RingSpawnChance = 0.025f,
                RingRotSpeed = 0.06f,
                RingDriftSpeed = 0.1f,
                RingColor = new Color(255, 200, 60, 160),

                MaxDust = 30,
                DustLife = 25,
                DustSize = 0.2f,
                DustSpawnChance = 0.28f,
                DustDriftSpeed = 0.4f,
                DustColor = new Color(255, 210, 80, 180),

                AutoDraw = true,
                SuppressDefaultDraw = true,
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
            Projectile.timeLeft = 300;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1.5f, 3.5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.GoldFlame, vel, 0,
                    new Color(255, 220, 100, 200),
                    Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, vel, 0,
                    new Color(255, 200, 80, 180), Main.rand.NextFloat(0.4f, 0.9f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
