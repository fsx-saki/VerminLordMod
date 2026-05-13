using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class YinYangBaseProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.45f, 0.7f),
            });

            Behaviors.Add(new YinYangTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.38f,
                GhostMaxPositions = 10,
                GhostWidthScale = 0.2f,
                GhostLengthScale = 1.5f,
                GhostColor = new Color(200, 195, 240, 180),

                MaxOrbs = 18,
                OrbLife = 32,
                OrbSize = 0.45f,
                OrbSpawnInterval = 2,
                OrbRotSpeed = 0.08f,
                OrbDriftSpeed = 0.2f,
                OrbSpread = 5f,
                OrbYinColor = new Color(60, 50, 100, 220),
                OrbYangColor = new Color(230, 225, 255, 220),

                MaxFish = 7,
                FishLife = 48,
                FishSize = 0.5f,
                FishSpawnChance = 0.035f,
                FishRotSpeed = 0.12f,
                FishDriftSpeed = 0.15f,
                FishYinColor = new Color(50, 40, 90, 200),
                FishYangColor = new Color(220, 215, 250, 200),

                MaxSCurves = 12,
                SCurveLife = 45,
                SCurveSize = 0.5f,
                SCurveAmplitude = 12f,
                SCurveSpawnChance = 0.06f,
                SCurveRotSpeed = 0.05f,
                SCurveDriftSpeed = 0.1f,
                SCurveYinColor = new Color(70, 55, 120, 180),
                SCurveYangColor = new Color(210, 200, 245, 180),

                MaxOrbitDots = 24,
                OrbitDotLife = 30,
                OrbitDotSize = 0.3f,
                OrbitDotRadius = 20f,
                OrbitDotAngularSpeed = 0.08f,
                OrbitDotSpawnChance = 0.15f,
                OrbitDotYinColor = new Color(80, 65, 140, 220),
                OrbitDotYangColor = new Color(240, 235, 255, 220),

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
                bool isYang = Main.rand.NextBool();
                Dust d = Dust.NewDustPerfect(
                    target.Center + Main.rand.NextVector2Circular(8f, 8f),
                    isYang ? DustID.PurpleTorch : DustID.Shadowflame, vel, 0,
                    isYang ? new Color(220, 215, 255, 200) : new Color(60, 50, 100, 200),
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
                bool isYang = Main.rand.NextBool();
                Dust d = Dust.NewDustPerfect(Projectile.Center,
                    isYang ? DustID.PurpleTorch : DustID.Shadowflame, vel, 0,
                    isYang ? new Color(200, 195, 240, 180) : new Color(50, 40, 90, 180),
                    Main.rand.NextFloat(0.4f, 0.9f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
