using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class WanLiSiLangProj : ModProjectile
    {
        private bool Stuck => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
            Projectile.knockBack = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            if (Stuck)
            {
                Projectile.velocity = Vector2.Zero;

                if (Main.rand.NextBool(6))
                {
                    var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Web);
                    d.noGravity = true;
                    d.velocity *= 0.2f;
                    d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                }

                Lighting.AddLight(Projectile.Center, 0.15f, 0.25f, 0.1f);
            }
            else
            {
                Projectile.rotation += 0.1f;

                if (Main.rand.NextBool(3))
                {
                    var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Silk);
                    d.noGravity = true;
                    d.velocity *= 0.3f;
                    d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (!Stuck)
            {
                Projectile.ai[0] = 1f;
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 300);
            target.AddBuff(BuffID.Webbed, 120);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Stuck)
            {
                float dist = Vector2.Distance(Projectile.Center, target.Center);
                if (dist > 30f)
                    return false;
            }

            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
}
