using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ChaoQiBuff : ModBuff
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
            bool isDay = Main.dayTime;

            if (isDay)
            {
                player.GetDamage(DamageClass.Generic) += 0.15f;
                player.moveSpeed += 0.10f;
                player.GetModPlayer<QiResourcePlayer>().ExtraQiRegen += 0.05f;

                if (Main.rand.NextBool(6))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Sunflower);
                    d.velocity *= 0.3f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                }
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.05f;

                if (Main.rand.NextBool(10))
                {
                    var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MothronEgg);
                    d.velocity *= 0.2f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.4f, 0.7f);
                }
            }
        }
    }
}
