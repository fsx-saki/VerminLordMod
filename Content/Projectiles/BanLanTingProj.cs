using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    public class BanLanTingProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 64;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (!proj.active || proj.whoAmI == Projectile.whoAmI)
                    continue;
                if (proj.friendly)
                    continue;
                if (!proj.hostile)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, proj.Center);
                if (dist < 40f)
                {
                    proj.Kill();
                    for (int j = 0; j < 5; j++)
                    {
                        var d = Dust.NewDustDirect(proj.position, proj.width, proj.height, DustID.BlueTorch);
                        d.noGravity = true;
                        d.velocity *= 0.5f;
                        d.scale = 1.2f;
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override Color? GetAlpha(Color lightColor) => new Color(80, 140, 255, 120);
    }
}
