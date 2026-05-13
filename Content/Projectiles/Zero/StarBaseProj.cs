using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    public class StarBaseProj : BaseBullet
    {
        private const float FlySpeed = 10f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.4f, 0.8f),
            });

            Behaviors.Add(new StarTrailBehavior
            {
                EnableGhostTrail = true,
                GhostAlpha = 0.35f,
                GhostMaxPositions = 10,
                GhostWidthScale = 0.2f,
                GhostLengthScale = 1.5f,
                GhostColor = new Color(180, 170, 230, 180),

                MaxStarPoints = 30,
                StarLife = 45,
                StarSize = 0.5f,
                StarSpawnInterval = 2,
                StarDriftSpeed = 0.35f,
                StarSpread = 5f,
                StarColor = new Color(220, 215, 255, 230),

                LineMaxDistance = 50f,
                LineBreakDistance = 75f,
                LineBaseAlpha = 0.3f,
                LineColor = new Color(180, 175, 230, 200),

                MaxNebula = 6,
                NebulaLife = 55,
                NebulaStartSize = 0.3f,
                NebulaEndSize = 1.8f,
                NebulaSpawnChance = 0.05f,
                NebulaDriftSpeed = 0.12f,
                NebulaColor = new Color(130, 110, 200, 120),

                MaxStardust = 35,
                StardustLife = 30,
                StardustSize = 0.2f,
                StardustSpawnChance = 0.35f,
                StardustDriftSpeed = 0.4f,
                StardustColor = new Color(200, 195, 255, 180),

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
                    DustID.PurpleTorch, vel, 0,
                    new Color(200, 190, 255, 200),
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
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0,
                    new Color(190, 180, 255, 180), Main.rand.NextFloat(0.4f, 0.9f));
                d.noGravity = true;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
