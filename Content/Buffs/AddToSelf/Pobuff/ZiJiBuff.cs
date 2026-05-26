using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZiJiBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.03f;
            player.statDefense += (int)(player.statDefense * 0.03f);
            player.moveSpeed += 0.03f;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Ambient_DarkBrown);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }
}
