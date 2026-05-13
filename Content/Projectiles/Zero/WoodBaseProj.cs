using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WoodBaseProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const float TrackWeight = 1f / 15f;
        private const int MaxLife = 240;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 600f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new GrassTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.32f,
                GhostMaxPositions = 10,
                GhostWidthScale = 0.2f,
                GhostLengthScale = 1.5f,
                GhostColor = new Color(80, 200, 60, 160),

                MaxLeaves = 28,
                LeafLife = 38,
                LeafSize = 0.5f,
                LeafSpawnInterval = 2,
                LeafRotSpeed = 0.06f,
                LeafDriftSpeed = 0.25f,
                LeafSpread = 5f,
                LeafColor = new Color(80, 200, 60, 210),

                MaxPollen = 30,
                PollenLife = 28,
                PollenSize = 0.25f,
                PollenSpawnChance = 0.22f,
                PollenDriftSpeed = 0.35f,
                PollenColor = new Color(200, 230, 80, 200),

                MaxBlooms = 4,
                BloomLife = 48,
                BloomStartSize = 0.3f,
                BloomEndSize = 1.4f,
                BloomSpawnChance = 0.018f,
                BloomDriftSpeed = 0.08f,
                BloomColor = new Color(150, 230, 120, 140),

                VineMaxDistance = 45f,
                VineBreakDistance = 70f,
                VineBaseAlpha = 0.25f,
                VineColor = new Color(60, 160, 50, 180),

                AutoDraw = true,
                SuppressDefaultDraw = true,
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 10,
                SpeedMin = 2f,
                SpeedMax = 5f,
                SpreadRadius = 5f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Grass,
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
            target.AddBuff(BuffID.Poisoned, 180);
            target.AddBuff(BuffID.Slow, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Grass, vel, 0,
                    new Color(30, 150, 30, 150), Main.rand.NextFloat(0.4f, 0.7f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
