using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Players
{
    public class XingNianXianPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Player.HasBuff(ModContent.BuffType<global::VerminLordMod.Content.Buffs.AddToSelf.Pobuff.XingNianXianBuff>()))
            {
                if (!Main.dayTime && Main.rand.NextBool(8))
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.AncientLight);
                    d.velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -1f);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.5f, 0.9f);
                }
            }
        }
    }
}
