using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class HuanJingCloneProj : ModProjectile
    {
        private const float AttractRange = 400f;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.damage = 0;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            Vector2 toOwner = owner.Center - Projectile.Center;
            float distToOwner = toOwner.Length();

            if (distToOwner > 250f)
            {
                toOwner.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * 5f, 0.05f);
            }
            else
            {
                Projectile.velocity *= 0.95f;
                Projectile.velocity += new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.3f, 0.3f));
            }

            Projectile.spriteDirection = owner.direction;
            Projectile.rotation = owner.velocity.X * 0.05f;

            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.townNPC)
                        continue;

                    float distToClone = Vector2.Distance(Projectile.Center, npc.Center);
                    float distToPlayer = Vector2.Distance(owner.Center, npc.Center);

                    if (distToClone < AttractRange && distToClone < distToPlayer)
                    {
                        npc.target = -1;
                        Vector2 dir = Projectile.Center - npc.Center;
                        dir.Normalize();
                        npc.velocity += dir * 0.5f;
                    }
                }
            }

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 150, 255, 100);
        }
    }
}
