using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class LongJuanProj : BaseBullet
    {
        private const float FlySpeed = 7f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.8f, 0.7f)
            });

            Behaviors.Add(new WindTrailBehavior
            {
                EnableGhostTrail = true,
                GhostMaxPositions = 8,
                GhostAlpha = 0.4f,
                GhostColor = new Color(160, 240, 220, 180),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 6f;
        }

        protected override void OnAI()
        {
            float angle = 0.1f;
            float speed = Projectile.velocity.Length();
            if (speed > 0f)
            {
                float currentAngle = Projectile.velocity.ToRotation();
                currentAngle += angle;
                Projectile.velocity = currentAngle.ToRotationVector2() * speed;
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 pushDir = (target.Center - owner.Center).SafeNormalize(Vector2.Zero);
            target.velocity += pushDir * 5f;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
