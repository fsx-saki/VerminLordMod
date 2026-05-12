using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Dusts
{
    public class IceBlizzardSnowDust : ModDust
    {
        public override string Texture => "VerminLordMod/Content/Trails/IceTrail/IceTrailSnow";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = false;
            dust.alpha = 0;
            dust.color = new Color(200, 240, 255);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.frame = new Rectangle(0, 0, 8, 8);
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.08f;
            dust.velocity *= 0.96f;
            dust.rotation += dust.velocity.X * 0.03f;
            dust.alpha += 4;
            dust.scale *= 0.99f;

            if (dust.alpha >= 255 || dust.scale < 0.05f)
                dust.active = false;

            return false;
        }
    }
}