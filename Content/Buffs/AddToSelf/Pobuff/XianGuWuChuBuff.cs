using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class XianGuWuChuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 5;
            player.lifeRegen += 2;

            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(15f, 30f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.AncientLight);
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.velocity.Y -= 0.3f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
