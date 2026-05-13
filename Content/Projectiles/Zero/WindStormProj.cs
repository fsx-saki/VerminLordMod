using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class WindStormProj : BaseBullet
    {
        private const float AccelMagnitude = 0.35f;
        private const float MaxSpeed = 11f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ConstrainedSteerBehavior(AccelMagnitude, MaxSpeed)
            {
                TrackMouse = true,
                Range = 0f,
                ConeHalfAngle = MathHelper.Pi / 5f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.6f, 0.5f),
            });

            Behaviors.Add(new WindTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.3f,
                GhostMaxPositions = 15,
                MaxStreaks = 55,
                StreakLife = 18,
                StreakSize = 0.5f,
                StreakStretch = 2.8f,
                StreakDrift = 0.3f,
                MaxVortex = 30,
                VortexLife = 25,
                VortexSize = 0.4f,
                VortexRotSpeed = 0.12f,
                MaxMist = 12,
                MistLife = 35,
                MistSpawnChance = 0.12f,
                AutoDraw = true,
                SuppressDefaultDraw = true,
            });

            Behaviors.Add(new PeriodicDustBehavior
            {
                SpawnChance = 0.4f,
                DustType = DustID.Cloud,
                SpreadRadius = 12f,
                Speed = 0.8f,
                ScaleMin = 0.3f,
                ScaleMax = 0.6f,
                Color = new Color(180, 240, 220, 150),
                NoGravity = true,
            });

            Behaviors.Add(new DustKillBehavior(
                dustType: DustID.Cloud,
                dustCount: 15,
                dustSpeed: 4f,
                dustScale: 1.0f
            )
            {
                NoGravity = true
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 300;
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

            for (int i = 0; i < 4; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 3f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Cloud, vel, 0,
                    new Color(180, 240, 220, 180),
                    Main.rand.NextFloat(0.4f, 0.8f));
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft) { }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
