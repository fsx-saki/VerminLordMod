using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class VoidBaseProj : BaseBullet
    {
        private const float FlySpeed = 7f;
        private const float TrackWeight = 1f / 25f;
        private const int MaxLife = 300;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 800f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            Behaviors.Add(new PullBehavior(pullRange: 200f, pullStrength: 0.12f, tangentFactor: 0.3f)
            {
                MaxPullSpeed = 6f,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.05f, 0.4f)
            });

            Behaviors.Add(new VoidTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.32f,
                GhostMaxPositions = 10,
                GhostWidthScale = 0.2f,
                GhostLengthScale = 1.5f,
                GhostColor = new Color(80, 20, 130, 160),

                MaxOrbs = 22,
                OrbLife = 38,
                OrbSize = 0.5f,
                OrbSpawnInterval = 2,
                OrbRotSpeed = 0.06f,
                OrbDriftSpeed = 0.2f,
                OrbSpread = 5f,
                OrbColor = new Color(80, 15, 140, 220),

                MaxDistortions = 4,
                DistortionLife = 48,
                DistortionStartSize = 0.3f,
                DistortionEndSize = 1.8f,
                DistortionSpawnChance = 0.018f,
                DistortionRotSpeed = 0.05f,
                DistortionDriftSpeed = 0.08f,
                DistortionColor = new Color(90, 20, 130, 160),

                MaxShards = 18,
                ShardLife = 22,
                ShardSize = 0.4f,
                ShardSpawnChance = 0.13f,
                ShardSpinSpeed = 0.12f,
                ShardDriftSpeed = 0.3f,
                ShardColor = new Color(70, 15, 110, 200),

                TendrilMaxDistance = 50f,
                TendrilBreakDistance = 80f,
                TendrilBaseAlpha = 0.2f,
                TendrilColor = new Color(70, 15, 110, 180),

                AutoDraw = true,
                SuppressDefaultDraw = true,
            });

            Behaviors.Add(new SplashBehavior(SplashMode.Ring)
            {
                Count = 12,
                SpeedMin = 3f,
                SpeedMax = 8f,
                SpreadRadius = 5f,
                RingAngleOffset = 0.3f,
                SpawnExtraDust = true,
                ExtraDustCount = 16,
                DustType = DustID.Shadowflame,
                DustColorStart = new Color(120, 40, 180, 220),
                DustColorEnd = new Color(40, 10, 60, 0),
                DustScaleMin = 0.4f,
                DustScaleMax = 0.9f,
                DustSpeedMin = 1f,
                DustSpeedMax = 5f,
                DustNoGravity = true,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 240);
            target.AddBuff(BuffID.Blackout, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 14; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, vel, 0,
                    new Color(120, 40, 180, 200), Main.rand.NextFloat(0.5f, 1.0f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}
