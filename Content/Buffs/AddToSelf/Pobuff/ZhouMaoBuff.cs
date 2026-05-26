using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZhouMaoBuff : ModBuff
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
            Lighting.AddLight(player.Center, new Vector3(0.5f, 0.45f, 0.2f));

            bool isDay = Main.dayTime;

            if (isDay)
            {
                player.GetDamage(DamageClass.Generic) += 0.12f;
                player.statDefense += 8;

                if (Main.rand.NextBool(5))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Sunflower);
                    d.velocity *= 0.3f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.7f, 1.1f);
                }
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.05f;

                if (Main.rand.NextBool(6))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch);
                    d.velocity *= 0.2f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                }
            }
        }
    }
}
