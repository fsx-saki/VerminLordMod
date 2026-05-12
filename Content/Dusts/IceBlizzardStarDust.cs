using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Dusts
{
    public class IceBlizzardStarDust : ModDust
    {
        public override string Texture => "VerminLordMod/Content/Trails/IceTrail/IceTrailStar";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.alpha = 0;
            dust.color = new Color(180, 230, 255);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.frame = new Rectangle(0, 0, 10, 10);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.95f;
            dust.rotation += 0.02f;
            dust.alpha += 5;
            dust.scale *= 0.98f;

            if (dust.alpha >= 255 || dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }
}