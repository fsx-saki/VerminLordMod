using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class FengManLouProj : ModProjectile
    {
        private const float PullRange = 300f;
        private const float PullStrength = 6f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            if (Main.rand.NextBool(2))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(30f, PullRange);
                var pos = Projectile.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos, 4, 4, DustID.Cloud);
                d.noGravity = true;
                d.velocity = (Projectile.Center - pos).SafeNormalize(Vector2.Zero) * 3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.2f);
            }

            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.townNPC)
                        continue;

                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < PullRange && dist > 10f)
                    {
                        Vector2 dir = Projectile.Center - npc.Center;
                        dir.Normalize();
                        float force = PullStrength * (1f - dist / PullRange);
                        npc.velocity += dir * force;
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(150, 220, 255, 120);
        }
    }
}
