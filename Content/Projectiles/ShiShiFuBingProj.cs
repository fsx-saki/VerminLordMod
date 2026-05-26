using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class ShiShiFuBingProj : ModProjectile
    {
        private const int RevealRadius = 300;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist <= RevealRadius)
                {
                    npc.velocity *= 0.99f;

                    if (Main.rand.NextBool(10))
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.IceTorch);
                        d.noGravity = true;
                        d.velocity *= 0.2f;
                        d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                    }
                }
            }

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                d.noGravity = true;
                d.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1.5f));
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }

            float playerX = Main.player[Projectile.owner].position.X;
            float playerY = Main.player[Projectile.owner].position.Y;
            float projX = Projectile.position.X;
            float projY = Projectile.position.Y;
            float playerWidth = Main.player[Projectile.owner].width;
            float playerHeight = Main.player[Projectile.owner].height;

            Player p = Main.player[Projectile.owner];
            if (p.position.X + p.width > Projectile.position.X &&
                p.position.X < Projectile.position.X + Projectile.width &&
                p.position.Y + p.height > Projectile.position.Y - 2f &&
                p.position.Y + p.height < Projectile.position.Y + 8f &&
                p.velocity.Y >= 0f &&
                p.gravDir == 1f)
            {
                p.position.Y = Projectile.position.Y - p.height;
                p.velocity.Y = 0f;
                p.fallStart = (int)(p.position.Y / 16f);
                p.jump = 0;
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
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch,
                    Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 0f), 0, default, 1.2f);
                d.noGravity = true;
            }
        }
    }
}
