using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class ShenYouProj : ModProjectile
    {
        private const float FollowSpeed = 12f;
        private const float SmoothWeight = 4f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            Vector2 toMouse = Main.MouseWorld - Projectile.Center;
            Vector2 targetVelocity = Vector2.Zero;

            if (toMouse != Vector2.Zero)
            {
                targetVelocity = toMouse.SafeNormalize(Vector2.Zero) * FollowSpeed;
            }

            if (targetVelocity != Vector2.Zero)
            {
                Projectile.velocity = (targetVelocity + Projectile.velocity * SmoothWeight) / (SmoothWeight + 1f);
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);

            if (Main.rand.NextBool(2))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 261);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ghost);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, 261);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 261);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
            }
        }
    }
}
