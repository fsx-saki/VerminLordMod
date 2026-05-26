using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShuiZhuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.waterWalk = true;
            player.gills = true;

            if (player.wet && !player.lavaWet && !player.honeyWet)
            {
                player.moveSpeed += 0.30f;
                player.maxRunSpeed += 0.30f;
            }

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Water);
                d.velocity *= 0.3f;
                d.noGravity = true;
            }
        }
    }
}
